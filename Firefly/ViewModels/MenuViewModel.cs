using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Firefly.Factories;
using Firefly.Helpers;
using Firefly.Models.Messages;
using Firefly.Services.Abstractions;
using Firefly.Views;

using HandyControl.Controls;
using HandyControl.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

using HcMessageBox = HandyControl.Controls.MessageBox;

namespace Firefly.ViewModels;

public partial class MenuViewModel : ObservableRecipient
{
    #region Fields

    private readonly IConfiguration _configuration;

    private readonly Dispatcher _dispatcher;

    private readonly IFireTableColumnMappingWindowFactory _fireTableColumnMappingWindowFactory;

    private readonly IViewSwitcher _viewSwitcher;

    #endregion Fields

    #region Properties

    public static IEnumerable<FontFamily> InstalledFontCollection => new InstalledFontCollection().Families
        .OrderBy(f => f.Name.Any(c => c is >= (char)0x4E00 and <= (char)0x9FFF) ? 0 : 1)
        .ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase);

    public bool CanOpenDocument => FireflyViewModel.IsFileSelected;

    public bool CanOpenDocumentFolder => FireflyViewModel.IsFileSelected;

    public bool CanOpenFFDocument => !String.IsNullOrEmpty(FireflyViewModel.OutputFilePath);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCccfDbMergeWindowToolTip))]
    [NotifyCanExecuteChangedFor(nameof(ShowCccfDbMergeWindowCommand))]
    public partial bool CanShowCccfDbMergeWindow { get; private set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCccfScraperWindowToolTip))]
    [NotifyCanExecuteChangedFor(nameof(ShowCccfScraperWindowCommand))]
    public partial bool CanShowCccfScraperWindow { get; private set; } = true;

    public CccfMainQueryViewModel CccfMainQueryViewModel { get; }

    public string CccfQueryUrl { get; }

    public FireflyViewModel FireflyViewModel { get; }

    public bool IsLocalModeAllowed { get; }

    public string? LocalModeDisallowedToolTip => IsLocalModeAllowed ? null : "\n\n* 暂未对普通用户开放";

    public MainViewModel MainViewModel { get; }

    public string OpenFFDocumentHeader => FireflyViewModel.OutputOrder > 0 ? $"打开 FF{FireflyViewModel.OutputOrder}__产品清单(_B)" : "打开 FF__产品清单(_B)";

    public string ShowCccfDbMergeWindowToolTip => CanShowCccfDbMergeWindow ? $"合并两个或更多本地数据库{LocalModeDisallowedToolTip}" : "程序已经在运行中...";

    public string ShowCccfScraperWindowToolTip => CanShowCccfScraperWindow ? $"同步 CCCF 服务器数据库中的产品信息到本地数据库{LocalModeDisallowedToolTip}" : "程序已经在运行中...";

    public VersionInfoViewModel VersionInfoViewModel { get; }

    #endregion Properties

    #region Constructors & Recipients

    public MenuViewModel(MainViewModel mainViewModel, FireflyViewModel fireflyViewModel, CccfMainQueryViewModel cccfMainQueryViewModel, VersionInfoViewModel versionInfoViewModel, IFireTableColumnMappingWindowFactory fireTableColumnMappingWindowFactory, IViewSwitcher viewSwitcher, IConfiguration configuration)
    {
        _dispatcher = Dispatcher.CurrentDispatcher;

        MainViewModel = mainViewModel;
        FireflyViewModel = fireflyViewModel;
        CccfMainQueryViewModel = cccfMainQueryViewModel;
        VersionInfoViewModel = versionInfoViewModel;

        _fireTableColumnMappingWindowFactory = fireTableColumnMappingWindowFactory;
        _viewSwitcher = viewSwitcher;
        _configuration = configuration;

        IsLocalModeAllowed = _configuration.GetValue("Cccf:LocalMode", false);

        FireflyViewModel.PropertyChanged += FireflyViewModel_PropertyChanged;
        CanShowCccfScraperWindow = !IsCccfWindowOpened(CccfScraperViewModel.Title, CccfScraperWindowProcess_Exited);
        CanShowCccfDbMergeWindow = !IsCccfWindowOpened(CccfDbMergeViewModel.Title, CccfDbMergeWindowProcess_Exited);

        CccfQueryUrl = $"{_configuration["CccfApi:BaseUrl"]}{_configuration["CccfApi:Endpoints:Query"]}";
    }

    #endregion Constructors & Recipients

    #region Commands

    [RelayCommand]
    private static void About()
    {
        var window = App.Current.Services.GetRequiredService<AboutWindow>();
        window.ShowDialog();
    }

    [RelayCommand]
    private static async Task DisclaimerAsync()
    {
        string disclaimer = await ResourceHelper.ReadAllTextAsync("免责声明.txt");
        HcMessageBox.Show(disclaimer, "免责声明");
    }

    [RelayCommand]
    private static void OpenAppSettings()
    {
        var result = HcMessageBox.Warning("""
            请勿修改，除非你确定知道这些配置的功能。

            应用程序的所有设置都包含在名为 appsettings.json 的文件中。对 appsettings.json 文件的任何更改都需要重新启动应用程序才能生效。
            """, App.AppName);

        if (result == MessageBoxResult.OK)
        {
            OpenRelativeFile(App.AppSettingsFileName);
        }
    }

    [RelayCommand]
    private static void OpenFireTableColumnMappingFolder()
    {
        var dir = Directory.CreateDirectory("mappings");
        FileHelper.OpenFolder(dir.FullName);
    }

    [RelayCommand]
    private static void OpenLocalApplicationData()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firefly");
        FileHelper.OpenFolder(path);
    }

    [RelayCommand]
    private static void OpenRelativeFile(string path)
    {
        FileHelper.OpenFile(path, true);
    }

    [RelayCommand]
    private static void OpenRelativeFolder(string path)
    {
        if (!String.IsNullOrWhiteSpace(path))
        {
            Directory.CreateDirectory(path);
        }

        FileHelper.OpenFolder(path, true);
    }

    [RelayCommand]
    private void ActivateKeyboardShortcuts()
    {
        Messenger.Send(new FocusOnMainWindowMessage());
    }

    [RelayCommand]
    private async Task EditCustomFireTableColumnMappingAsync()
    {
        var result = _fireTableColumnMappingWindowFactory.ShowDialog(FireflyViewModel.ColumnMapping, FireflyViewModel.CustomColumnMappingFilePath);

        if (result.DialogResult is true)
        {
            FireflyViewModel.ColumnMapping = result.ColumnMapping;
            await await Messenger.Send(new AsyncTaskRequestMessage<string?, bool>(result.FilePath), "LoadFireTableColumnMapping");
        }
        else if (result.DialogResult is null)
        {
            await await Messenger.Send(new AsyncTaskRequestMessage<string?, bool>(null), "LoadFireTableColumnMapping");
        }
    }

    [RelayCommand]
    private async Task EditPresetFireTableColumnMappingAsync()
    {
        HcMessageBox.Info("预设的表列映射是只读的，你可以编辑副本并使用。", App.AppName);

        var result = _fireTableColumnMappingWindowFactory.ShowDialog(FireflyViewModel.PresetColumnMapping, null);

        if (result.DialogResult is true)
        {
            await await Messenger.Send(new AsyncTaskRequestMessage<string?, bool>(result.FilePath), "LoadFireTableColumnMapping");
        }
        else if (result.DialogResult is null)
        {
            await await Messenger.Send(new AsyncTaskRequestMessage<string?, bool>(null), "LoadFireTableColumnMapping");
        }
    }

    [RelayCommand]
    private async Task LoadFireTableColumnMappingAsync()
    {
        var dir = Directory.CreateDirectory("mappings");

        var dialog = new OpenFileDialog {
            DefaultDirectory = dir.FullName,
            InitialDirectory = dir.FullName,
            DefaultExt = ".firemap",
            Filter = "表列映射 (*.firemap)|*.firemap"
        };

        if (dialog.ShowDialog() is true)
        {
            string filename = dialog.FileName;
            await await Messenger.Send(new AsyncTaskRequestMessage<string?, bool>(filename), "LoadFireTableColumnMapping");
        }
    }

    [RelayCommand]
    private async Task LoadPresetFireTableColumnMappingAsync()
    {
        if (FireflyViewModel.IsCustomColumnMappingLoaded)
        {
            await await Messenger.Send(new AsyncTaskRequestMessage<string?, bool>(null), "LoadFireTableColumnMapping");
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenDocument))]
    private void OpenDocument()
    {
        FileHelper.OpenFile(FireflyViewModel.InputFilePath);
    }

    [RelayCommand(CanExecute = nameof(CanOpenDocumentFolder))]
    private void OpenDocumentFolder()
    {
        FileHelper.OpenFolder(FireflyViewModel.InputFilePath);
    }

    [RelayCommand(CanExecute = nameof(CanOpenFFDocument))]
    private void OpenFFDocument()
    {
        FileHelper.OpenFile(FireflyViewModel.OutputFilePath);
    }

    [RelayCommand]
    private void ResetFireTableAutoColumnWidths()
    {
        Messenger.Send(new ActionMessage(), "ResetFireTableAutoColumnWidths");
    }

    [RelayCommand(CanExecute = nameof(CanShowCccfDbMergeWindow))]
    private void ShowCccfDbMergeWindow()
    {
        CanShowCccfDbMergeWindow = false;
        var window = App.Current.Services.GetRequiredService<CccfDbMergeWindow>();
        window.Closed += CccfDbMergeWindow_Closed;
        window.Show();
    }

    [RelayCommand(CanExecute = nameof(CanShowCccfScraperWindow))]
    private void ShowCccfScraperWindow()
    {
        var result = HcMessageBox.Info(
             "「同步本地数据库」是一种网络爬虫工具，请您在使用时严格遵守相关法律法规以及 CCCF 网站的使用条款。",
             App.AppName);

        if (result != MessageBoxResult.OK)
        {
            return;
        }

        CanShowCccfScraperWindow = false;
        var window = App.Current.Services.GetRequiredService<CccfScraperWindow>();
        // 添加 Loaded InvokeCommandAction 行为无效，可能是手动创建/打开窗口的原因？
        window.Loaded += CccfScraperWindow_Loaded;
        window.Closed += CccfScraperWindow_Closed;
        window.Show();
    }

    [RelayCommand]
    private async Task ShowFireTableColumnMappingWindowAsync()
    {
        var result = _fireTableColumnMappingWindowFactory.ShowDialog();

        if (result.DialogResult is true)
        {
            var tcs = new TaskCompletionSource<bool>();

            var growlInfo = new GrowlInfo {
                Message = $"表列映射保存成功。\n> {PathHelper.GetRelativePathOrOriginal(result.FilePath)}\n\n是否立即加载？",
                ShowDateTime = true,
                ConfirmStr = "加载",
                CancelStr = "取消",
                IsCustom = true,
                IconKey = ResourceToken.AskGeometry,
                IconBrushKey = ResourceToken.SuccessBrush,
                ActionBeforeClose = isConfirm => {
                    if (isConfirm)
                    {
                        tcs.TrySetResult(true);
                    }

                    tcs.TrySetResult(false);

                    return true;
                }
            };

            Growl.Ask(growlInfo);

            bool loadConfirmed = await tcs.Task;

            if (loadConfirmed)
            {
                await await Messenger.Send(new AsyncTaskRequestMessage<string?, bool>(result.FilePath), "LoadFireTableColumnMapping");
            }
        }
    }

    [RelayCommand]
    private void ToggleFireTableGroupState(bool state)
    {
        if (FireflyViewModel.FireTableService.FireCheckContexts.Count > 0)
        {
            FireflyViewModel.AreAllGroupsExpanded = state;
        }
    }

    #endregion Commands

    #region Methods

    private static bool IsCccfWindowOpened(string title, EventHandler onExited)
    {
        var ps = Process.GetProcessesByName(App.AppName);
        var p = ps.FirstOrDefault(x => x.MainWindowTitle == title);

        if (p is not null)
        {
            p.EnableRaisingEvents = true;
            p.Exited += onExited;

            return true;
        }

        return false;
    }

    #endregion Methods

    #region Event Handlers

    private void CccfDbMergeWindow_Closed(object? sender, EventArgs e)
    {
        CanShowCccfDbMergeWindow = true;
    }

    private void CccfDbMergeWindowProcess_Exited(object? sender, EventArgs e)
    {
        _dispatcher.Invoke(() => CanShowCccfDbMergeWindow = true);
    }

    private void CccfScraperWindow_Closed(object? sender, EventArgs e)
    {
        CanShowCccfScraperWindow = true;
    }

    private async void CccfScraperWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var window = (CccfScraperWindow)sender;
        await window.ViewModel.LoadedCommand.ExecuteAsync(null);
    }

    private void CccfScraperWindowProcess_Exited(object? sender, EventArgs e)
    {
        _dispatcher.Invoke(() => CanShowCccfScraperWindow = true);
    }

    private void FireflyViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FireflyViewModel.IsFileSelected))
        {
            OpenDocumentCommand.NotifyCanExecuteChanged();
            OpenDocumentFolderCommand.NotifyCanExecuteChanged();
        }

        if (e.PropertyName == nameof(FireflyViewModel.FireTaskStatus))
        {
            OpenFFDocumentCommand.NotifyCanExecuteChanged();
        }

        if (e.PropertyName == nameof(FireflyViewModel.OutputFilePath))
        {
            OpenFFDocumentCommand.NotifyCanExecuteChanged();
        }

        if (e.PropertyName == nameof(FireflyViewModel.OutputOrder))
        {
            OnPropertyChanged(nameof(OpenFFDocumentHeader));
        }
    }

    #endregion Event Handlers
}
