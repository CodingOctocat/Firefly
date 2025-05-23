using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Firefly.Extensions;
using Firefly.Factories;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Properties;
using Firefly.Services;
using Firefly.Services.Abstractions;
using Firefly.Services.Requests;

using HandyControl.Controls;
using HandyControl.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

using Polly.Timeout;

using HcMessageBox = HandyControl.Controls.MessageBox;

namespace Firefly.ViewModels;

public partial class FireflyViewModel : ObservableRecipient
{
    #region Fields

    private const string GroupDescription = "FireProduct.FireSystem.Name";

    private readonly IConfiguration _configuration;

    private readonly IFireTableColumnMappingWindowFactory _fireTableColumnMappingWindowFactory;

    private readonly IViewSwitcher _viewSwitcher;

    private TaskCompletionSource<bool>? _taskCanceledTcs;

    #endregion Fields

    #region Properties

    public static bool IsPresetColumnMappingAsDefault => String.IsNullOrWhiteSpace(Settings.Default.DefaultColumnMappingPath);

    [ObservableProperty]
    public partial bool AreAllGroupsExpanded { get; set; } = true;

    [ObservableProperty]
    public partial bool CanDragDrop { get; private set; } = true;

    public bool CanStart => IsDocumentLoaded && IsIdle;

    [ObservableProperty]
    public partial FireTableColumnMapping ColumnMapping { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomColumnMappingLoaded))]
    [NotifyPropertyChangedFor(nameof(IsLoadedCustomColumnMappingAsDefault))]
    public partial string? CustomColumnMappingFilePath { get; private set; }

    public string DocumentFolder => Path.GetDirectoryName(InputFilePath) ?? "";

    public FindInPageBarViewModel FindInPageBarViewModel { get; } = new();

    public CollectionViewSource FireCheckContextsCvs { get; }

    public ICollectionView FireCheckContextsView { get; }

    public IFireCheckSettings FireCheckSettings { get; }

    public FireTableColumnVisibility FireTableColumnVisibility { get; } = new();

    public FireTableService FireTableService { get; }

    [ObservableProperty]
    public partial FireTaskStatus FireTaskStatus { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFileSelected))]
    [NotifyPropertyChangedFor(nameof(DocumentFolder))]
    public partial string InputFilePath { get; private set; } = "";

    public bool IsBusy => LoadDocumentCommand.IsRunning || StartCommand.IsRunning;

    public bool IsCustomColumnMappingLoaded => CustomColumnMappingFilePath is not null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    public partial bool IsDocumentLoaded { get; private set; }

    [ObservableProperty]
    public partial bool IsDragging { get; private set; }

    public bool IsFileSelected => Path.Exists(InputFilePath);

    public bool IsIdle => !IsBusy;

    public bool IsLoadedCustomColumnMappingAsDefault => CustomColumnMappingFilePath == Settings.Default.DefaultColumnMappingPath;

    [ObservableProperty]
    public partial bool IsWriting { get; private set; }

    [ObservableProperty]
    public partial string OutputFilePath { get; private set; } = "";

    [ObservableProperty]
    public partial int OutputOrder { get; private set; }

    public FireTableColumnMapping PresetColumnMapping { get; }

    public ProgressTimer ProgressTimer { get; }

    public int QueryDelay => App.IsInDebugMode ? 50 : Math.Max(_configuration.GetValue("Firefly:QueryDelay", 500), 200);

    [ObservableProperty]
    public partial FireCheckContext? SelectedFireCheckContext { get; set; }

    [ObservableProperty]
    public partial TaskbarItemProgressState TaskbarItemProgressState { get; private set; }

    [ObservableProperty]
    public partial bool ViewErrorOnly { get; set; }

    #endregion Properties

    #region Constructors & Recipients

    public FireflyViewModel(
        FireTableService fireTableService,
        IFireCheckSettings fireCheckSettings,
        IFireTableColumnMappingWindowFactory fireTableColumnMappingWindowFactory,
        IConfiguration configuration,
        IViewSwitcher viewSwitcher,
        ProgressTimer progressTimer)
    {
        IsActive = true;

        FireTableService = fireTableService;

        FireCheckSettings = fireCheckSettings;
        _fireTableColumnMappingWindowFactory = fireTableColumnMappingWindowFactory;
        _configuration = configuration;

        FireCheckSettings.StrictMode = _configuration.GetValue("Firefly:StrictMode", true);
        FireCheckSettings.CheckReportNumber = _configuration.GetValue("Firefly:CheckReportNumber", false);

        var section = _configuration.GetSection("ColumnMappings:Preset");
        PresetColumnMapping = section.Get<FireTableColumnMapping>() ?? throw new InvalidOperationException($"Configuration section '{section.Path}' is missing or invalid for FireTableColumnMapping.");
        ColumnMapping = PresetColumnMapping;

        _viewSwitcher = viewSwitcher;
        ProgressTimer = progressTimer;

        FireCheckContextsCvs = new CollectionViewSource {
            Source = FireTableService.FireCheckContexts,
            IsLiveFilteringRequested = true,
            IsLiveGroupingRequested = true
        };

        FireCheckContextsCvs.LiveFilteringProperties.Add(nameof(FireCheckContext.HasError));
        FireCheckContextsCvs.GroupDescriptions.Add(new PropertyGroupDescription(GroupDescription));
        FireCheckContextsView = FireCheckContextsCvs.View;

        LoadDocumentCommand.PropertyChanged += (s, e) => NotifyUIStatus();
        StartCommand.PropertyChanged += (s, e) => NotifyUIStatus();
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        Messenger.Register<ValueChangedMessage<bool>, string>(this, "IsDragging",
            (r, m) => IsDragging = m.Value);

        Messenger.Register<RequestMessage<string>, string>(this, "DocumentPath",
            (r, m) => m.Reply(InputFilePath));

        Messenger.Register<AsyncTaskRequestMessage<string?, bool>, string>(this, "LoadFireTableColumnMapping",
            (r, m) => m.Reply(LoadFireTableColumnMappingAsync(m.Request)));
    }

    #endregion Constructors & Recipients

    #region Commands

    [RelayCommand]
    public void SetCustomColumnMappingAsDefault()
    {
        if (IsLoadedCustomColumnMappingAsDefault)
        {
            Settings.Default.DefaultColumnMappingPath = null;
        }
        else
        {
            Settings.Default.DefaultColumnMappingPath = CustomColumnMappingFilePath;
        }

        OnPropertyChanged(nameof(IsLoadedCustomColumnMappingAsDefault));
    }

    [RelayCommand]
    public void SetPresetColumnMappingAsDefault()
    {
        Settings.Default.DefaultColumnMappingPath = null;
        OnPropertyChanged(nameof(IsLoadedCustomColumnMappingAsDefault));
    }

    [RelayCommand]
    private async Task Cancel()
    {
        var result = HcMessageBox.Ask("你确定要结束任务？", App.AppName);

        if (result == MessageBoxResult.OK)
        {
            _taskCanceledTcs = new();
            StartCancelCommand.Execute(null);

            // 等待取消信息通知在主窗口显示后再询问是否导出，否则通知会在 MessageBox 上显示
            await _taskCanceledTcs.Task;

            var export = HcMessageBox.Show(
                "已完成部分进度。是否导出？",
                App.AppName,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (export == MessageBoxResult.Yes)
            {
                await WriteAsync();
            }
        }
    }

    [RelayCommand]
    private async Task LoadDocumentAsync(string? path)
    {
        path ??= InputFilePath;
        bool reload = path == InputFilePath;

        if (FireTableService.FireCheckContexts.Any(x => x.Cccf is not null))
        {
            string msg1 = reload ? "重新" : "";
            string msg2 = reload ? "" : $"\n> {path}";

            var result = HcMessageBox.Show(
                $"当前检查视图将被清除。是否仍要{msg1}加载文档？{msg2}",
                App.AppName,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        InputFilePath = path;

        Reset(true);

        TaskbarItemProgressState = TaskbarItemProgressState.Indeterminate;

        try
        {
            await FireTableService.ReadAsync(InputFilePath, ColumnMapping);

            IsDocumentLoaded = true;
            TaskbarItemProgressState = TaskbarItemProgressState.None;
            ProgressTimer.TotalTasks = FireTableService.FireCheckContexts.Count;

            bool isPreset = FireTableService.IsPresetFireTable();

            if (!isPreset && !IsCustomColumnMappingLoaded && FireTableService.FireCheckContexts.Count == 0)
            {
                HcMessageBox.Warning($"预设表列映射似乎未能正确读取文档内容，你可能需要创建或加载自定义的表列映射。如果一切正常，请忽略此警告。\n\n预设表列映射信息:\n{PresetColumnMapping}", App.AppName);
            }

            await UpdateFireTableColumnStateAsync();
        }
        catch (Exception)
        {
            TaskbarItemProgressState = TaskbarItemProgressState.Error;

            throw;
        }
    }

    [RelayCommand]
    private async Task LoadedAsync()
    {
        await GeoResources.ReadAsync();

        if (File.Exists(Settings.Default.DefaultColumnMappingPath))
        {
            await LoadFireTableColumnMappingAsync(Settings.Default.DefaultColumnMappingPath);
        }
    }

    [RelayCommand]
    private async Task ManualQueryFieldAsync(KeyValuePair<string, string>? cellInfo)
    {
        if (cellInfo is null)
        {
            return;
        }

        await _viewSwitcher.SwitchToAsync(ActiveView.CccfQuery);

        string name = "";
        string model = "";
        string ent = "";
        string certNo = "";
        string reportNo = "";

        if (cellInfo.Value.Key is "CCCF" or "设备名称")
        {
            name = cellInfo.Value.Value;
        }
        else if (cellInfo.Value.Key is "产品型号")
        {
            model = cellInfo.Value.Value;
        }
        else if (cellInfo.Value.Key is "生产厂家")
        {
            ent = cellInfo.Value.Value;
        }
        else if (cellInfo.Value.Key is "证书编号")
        {
            certNo = cellInfo.Value.Value;
        }
        else if (cellInfo.Value.Key is "检验报告")
        {
            reportNo = cellInfo.Value.Value;
        }

        var request = new CccfRequest(1, name, model, ent, certNo, reportNo);
        await await Messenger.Send(new ManualQueryAsyncRequestMessage(request));

        Messenger.Send(new FocusOnQueryBarMessage(false, false));
    }

    [RelayCommand]
    private async Task ManualQueryMoreAsync(CccfRequest cccfRequest)
    {
        await _viewSwitcher.SwitchToAsync(ActiveView.CccfQuery);
        await await Messenger.Send(new ManualQueryAsyncRequestMessage(cccfRequest));
    }

    [RelayCommand(CanExecute = nameof(IsIdle))]
    private async Task OpenFileDialogAsync()
    {
        var dialog = new OpenFileDialog {
            AddToRecent = true,
            CheckFileExists = true,
            DefaultExt = ".docx",
            Filter = "Word 文档 (*.docx)|*.docx",
        };

        if (dialog.ShowDialog() is true)
        {
            await LoadDocumentCommand.ExecuteAsync(dialog.FileName);
        }
    }

    [RelayCommand(CanExecute = nameof(IsDocumentLoaded))]
    private async Task ReloadDocumentAsync()
    {
        await LoadDocumentCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(CanStart), IncludeCancelCommand = true)]
    private async Task StartAsync(CancellationToken cancellationToken = default)
    {
        int startProgress = Math.Max(0, ProgressTimer.CurrentProgress - 1);

        if (FireTaskStatus is FireTaskStatus.Cancelled or FireTaskStatus.Error)
        {
            var result = HcMessageBox.Show(
                $"""
                上一次任务未完成 (状态: {FireTaskStatus.GetDescription()})。
                是否要继续任务？

                [是]: 从第 {startProgress + 1} 项开始
                [否]: 重新开始
                """,
                 App.AppName,
                 MessageBoxButton.YesNoCancel,
                 MessageBoxImage.Question,
                 MessageBoxResult.Yes);

            if (result == MessageBoxResult.Yes)
            {
                ProgressTimer.UpdateProgress(startProgress);
                Reset(false);
            }
            else if (result == MessageBoxResult.No)
            {
                Reset(true);
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        TaskbarItemProgressState = TaskbarItemProgressState.Normal;
        ProgressTimer.Start();

        var progress = new Progress<int>();
        progress.ProgressChanged += (_, e) => ProgressTimer.UpdateProgress(e);

        try
        {
            await FireTableService.CheckAsync(startProgress, QueryDelay, progress, cancellationToken);
            await WriteAsync();

            ProgressTimer.Stop(true);
            FireTaskStatus = FireTaskStatus.Completed;
            TaskbarItemProgressState = TaskbarItemProgressState.None;
        }
        catch (TaskCanceledException)
        {
            FireTaskStatus = FireTaskStatus.Cancelled;
            TaskbarItemProgressState = TaskbarItemProgressState.Paused;
            Growl.Info("任务被取消。");
            _taskCanceledTcs?.TrySetResult(true);
        }
        catch (HttpRequestException ex)
        {
            FireTaskStatus = FireTaskStatus.Error;
            TaskbarItemProgressState = TaskbarItemProgressState.Error;
            Growl.Error($"HTTP 请求错误: {ex.GetHttpStatusDescription()}");
        }
        catch (TimeoutRejectedException ex)
        {
            FireTaskStatus = FireTaskStatus.Error;
            TaskbarItemProgressState = TaskbarItemProgressState.Error;
            Growl.Error($"请求超时。({ex.Timeout.TotalSeconds}s)");
        }
        catch
        {
            FireTaskStatus = FireTaskStatus.Error;
            TaskbarItemProgressState = TaskbarItemProgressState.Error;

            throw;
        }
        finally
        {
            ProgressTimer.Stop();
        }
    }

    #endregion Commands

    #region DragDrop Commands

    [RelayCommand]
    private static void DragEnter(DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
        e.Handled = true;
    }

    [RelayCommand]
    private static void DragOver(DragEventArgs e)
    {
        Mouse.SetCursor(Cursors.None);
        e.Handled = true;
    }

    [RelayCommand]
    private static void DragOverBlock(DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        Mouse.SetCursor(Cursors.No);
        e.Handled = true;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task DropAsync(DragEventArgs e)
    {
        try
        {
            IsDragging = false;

            string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0].ToString();

            if (!CheckFileFormat(path))
            {
                return;
            }

            await _viewSwitcher.SwitchToAsync(ActiveView.Firefly);

            if (IsBusy)
            {
                Growl.Info($"任务进行中，请稍后重试。\n{InputFilePath}");

                return;
            }

            await LoadDocumentCommand.ExecuteAsync(path);
        }
        finally
        {
            e.Handled = true;
        }
    }

    [RelayCommand]
    private void DropBlock(DragEventArgs e)
    {
        IsDragging = false;
        e.Handled = true;
    }

    #endregion DragDrop Commands

    #region OnPropertyChanged

    partial void OnViewErrorOnlyChanged(bool value)
    {
        if (value)
        {
            FireCheckContextsView.Filter = FilterErrorItems;
        }
        else
        {
            FireCheckContextsView.Filter = null;
        }
    }

    #endregion OnPropertyChanged

    #region Methods

    private static bool CheckFileFormat(string path)
    {
        string ext = Path.GetExtension(path);

        if (ext == ".doc")
        {
            HcMessageBox.Show(
                $"无法打开 {Path.GetFileName(path)}。\n请将 .doc 文档转换为 .docx 格式。",
                App.AppName,
                MessageBoxButton.OK,
                MessageBoxImage.Stop,
                MessageBoxResult.OK);

            return false;
        }

        if (ext != ".docx")
        {
            HcMessageBox.Show(
                $"无法打开 {Path.GetFileName(path)}。\n格式不支持。",
                App.AppName,
                MessageBoxButton.OK,
                MessageBoxImage.Stop,
                MessageBoxResult.OK);

            return false;
        }

        return true;
    }

    private static bool FilterErrorItems(object item)
    {
        if (item is FireCheckContext fireCheckContext)
        {
            return fireCheckContext.HasError is true;
        }

        return false;
    }

    private void GetOutputFilePath()
    {
        string inputFileName = Path.GetFileName(InputFilePath);
        var inputFileNameMatch = InputFileNameRegex().Match(inputFileName);

        if (inputFileNameMatch.Success)
        {
            inputFileName = inputFileNameMatch.Groups[3].Value;
        }

        var files = Directory.EnumerateFiles(DocumentFolder);

        int maxOrder = 0;

        foreach (string file in files)
        {
            string filename = Path.GetFileName(file);
            var match = Regex.Match(filename, $@"FF(\d+)_{Regex.Escape(inputFileName)}");

            if (match.Success)
            {
                int order = Int32.Parse(match.Groups[1].Value);

                if (order > maxOrder)
                {
                    maxOrder = order;
                }
            }
        }

        OutputOrder = maxOrder + 1;
        OutputFilePath = Path.Combine(DocumentFolder, $"FF{OutputOrder}_{inputFileName}");
    }

    private async Task<bool> LoadFireTableColumnMappingAsync(string? path)
    {
        string? relativePath = PathHelper.GetRelativePathOrOriginal(path);

        try
        {
            if (path is null)
            {
                ColumnMapping = PresetColumnMapping;
                CustomColumnMappingFilePath = null;
                Growl.Info($"已加载预设表列映射。");
            }
            else
            {
                string json = await File.ReadAllTextAsync(path);
                ColumnMapping = JsonSerializer.Deserialize<FireTableColumnMapping>(json) ?? throw new JsonException("反序列化结果为 null。");
                CustomColumnMappingFilePath = path;

                if (!ColumnMapping.Validate())
                {
                    HcMessageBox.Error($"表列映射参数错误。\n> {relativePath}\n\n即将打开编辑器窗口进行修复...", App.AppName);
                    var result = _fireTableColumnMappingWindowFactory.ShowDialog(ColumnMapping, CustomColumnMappingFilePath);

                    // 保存
                    if (result.DialogResult is true)
                    {
                        ColumnMapping = result.ColumnMapping;
                        CustomColumnMappingFilePath = result.FilePath;
                    }
                    else if (result.DialogResult is false) // 关闭
                    {
                        Growl.Error($"表列映射参数修复失败。\n> {relativePath}");
                    }
                    else // 删除
                    {
                        ColumnMapping = PresetColumnMapping;
                        CustomColumnMappingFilePath = null;
                        SetPresetColumnMappingAsDefault();

                        Growl.Info($"已加载预设表列映射。");
                    }
                }
                else
                {
                    Growl.Info($"已加载表列映射。\n> {relativePath}");
                }
            }
        }
        catch (Exception ex)
        {
            HcMessageBox.Error($"表列映射加载错误: {relativePath}\n\n<{ex.GetType()}>\n{ex.Message}", App.AppName);

            return false;
        }

        if (ReloadDocumentCommand.CanExecute(null))
        {
            await ReloadDocumentCommand.ExecuteAsync(null);
        }

        return true;
    }

    private void NotifyUIStatus()
    {
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(IsIdle));
        OpenFileDialogCommand.NotifyCanExecuteChanged();
        StartCommand.NotifyCanExecuteChanged();
    }

    private void Reset(bool resetProgress)
    {
        if (resetProgress)
        {
            foreach (var fireCheckContext in FireTableService.FireCheckContexts)
            {
                fireCheckContext.Reset();
            }
        }

        ProgressTimer.Reset(resetProgress);
        FireTaskStatus = FireTaskStatus.Normal;
        TaskbarItemProgressState = TaskbarItemProgressState.None;
    }

    private async Task UpdateFireTableColumnStateAsync()
    {
        // 如果是预设表列映射，那么不显示 Count、Status 列（节省列空间）
        FireTableColumnVisibility.ProductName = ColumnMapping.ProductNameColumn != 0;
        FireTableColumnVisibility.Count = ColumnMapping.CountColumn != 0 && IsCustomColumnMappingLoaded;
        FireTableColumnVisibility.Model = ColumnMapping.ModelColumn != 0;
        FireTableColumnVisibility.EnterpriseName = ColumnMapping.EnterpriseNameColumn != 0;
        FireTableColumnVisibility.CertificateNumber = ColumnMapping.CertificateNumberColumn != 0;
        FireTableColumnVisibility.ReportNumber = ColumnMapping.ReportNumberColumn != 0;
        FireTableColumnVisibility.Status = ColumnMapping.StatusColumn != 0 && IsCustomColumnMappingLoaded;
        FireTableColumnVisibility.ManufactureDate = ColumnMapping.ManufactureDateColumn != 0;
        FireTableColumnVisibility.Remark = ColumnMapping.RemarkColumn != 0;

        // HACK: 必须执行两次 Yield 方法，以确保仅基于当前视口的列宽进行调整。否则，在重置自动列宽时，将考虑所有行的列宽
        await Dispatcher.Yield();
        await Dispatcher.Yield();

        Messenger.Send(new ActionMessage(), "ResetFireTableAutoColumnWidths");
    }

    private async Task WriteAsync()
    {
        IsWriting = true;
        TaskbarItemProgressState = TaskbarItemProgressState.Indeterminate;

        GetOutputFilePath();
        Growl.Info($"正在导出: {OutputFilePath}");

        try
        {
            await FireTableService.WriteAsync(OutputFilePath);
        }
        finally
        {
            IsWriting = false;
        }

        Growl.Ask(new GrowlInfo {
            Message = $"任务已完成。\n> {OutputFilePath}\n\n是否打开文档？",
            ShowDateTime = true,
            ConfirmStr = "打开",
            CancelStr = "稍后",
            IsCustom = true,
            IconKey = ResourceToken.SuccessGeometry,
            IconBrushKey = ResourceToken.SuccessBrush,
            ActionBeforeClose = isConfirm => {
                if (isConfirm)
                {
                    FileHelper.OpenFile(OutputFilePath);
                }

                return true;
            }
        });
    }

    #endregion Methods

    #region Regex

    [GeneratedRegex(@"(FF(\d+)_)?(.*)")]
    private static partial Regex InputFileNameRegex();

    #endregion Regex
}
