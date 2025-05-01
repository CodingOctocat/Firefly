using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Messages;

using GongSolutions.Wpf.DragDrop;

using Microsoft.Win32;

using HcMessageBox = HandyControl.Controls.MessageBox;

namespace Firefly.ViewModels;

public partial class FireTableColumnMappingViewModel : ObservableRecipient, IDropTarget
{
    #region Fields

    private bool _confirmClosing;

    #endregion Fields

    #region Properties

    public bool CanDelete => File.Exists(MappingFilePath);

    public FireTableColumnMapping ColumnMapping { get; }

    /// <summary>
    /// <see langword="true"/>: 保存；<see langword="false"/>: 关闭；<see langword="null"/>: 删除。
    /// </summary>
    public bool? DialogResult { get; private set; }

    public string DocumentPath { get; }

    public ComboBoxItemWrapper<FireTableColumnNumber>[] FireSystemColumnNumbers { get; } = ComboBoxItemWrapper.CreateByValueDescription<FireTableColumnNumber>();

    public string MappingFileName => String.IsNullOrWhiteSpace(MappingFilePath) ? "未命名" : Path.GetFileName(MappingFilePath);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MappingFileName))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    public partial string? MappingFilePath { get; private set; } = null!;

    public ComboBoxItemWrapper<FireTableColumnNumber>[] OptionalColumnNumbers { get; } = [.. ComboBoxItemWrapper.CreateByValueDescription<FireTableColumnNumber>().Skip(1)];

    public ComboBoxItemWrapper<FireTableColumnNumber>[] RequiredColumnNumbers { get; } = [.. ComboBoxItemWrapper.CreateByValueDescription<FireTableColumnNumber>().Skip(2)];

    [ObservableProperty]
    public partial ComboBoxItemWrapper<FireTableColumnNumber>[] RuleColumnsNumbers { get; private set; } = [];

    [ObservableProperty]
    public partial ComboBoxItemWrapper<FireTableColumnNumber>[] SkipRowRuleColumnsNumbers { get; private set; } = [];

    #endregion Properties

    #region Constructors

    public FireTableColumnMappingViewModel() : this(new(), null!)
    { }

    public FireTableColumnMappingViewModel(FireTableColumnMapping? columnMapping, string? mappingFilePath)
    {
        DocumentPath = Messenger.Send(new RequestMessage<string>(), "DocumentPath").Response;

        if (columnMapping is null)
        {
            ColumnMapping = new();
        }
        else
        {
            // 创建副本避免放弃保存时修改原始数据
            ColumnMapping = JsonSerializer.Deserialize<FireTableColumnMapping>(JsonSerializer.Serialize(columnMapping))!;
        }

        MappingFilePath = mappingFilePath!;
        UpdateColumnOptions();

        ColumnMapping.PropertyChanged += (s, e) => UpdateColumnOptions();
    }

    #endregion Constructors

    #region Commands

    [RelayCommand]
    private void AddSkipRowRule()
    {
        ColumnMapping.SkipRowRules.Add(new());
    }

    [RelayCommand]
    private async Task ClosingAsync(CancelEventArgs e)
    {
        if (_confirmClosing)
        {
            return;
        }

        if (ColumnMapping.IsChanged)
        {
            var result = HcMessageBox.Show(
                $"是否要将更改保存到 “{MappingFileName}”？",
                App.AppName,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            if (result == MessageBoxResult.Yes)
            {
                e.Cancel = true;

                if (ColumnMapping.HasError)
                {
                    HcMessageBox.Error("表列映射存在错误，请修复后再保存。", App.AppName);
                }
                else
                {
                    await SaveAsync();
                }
            }
            else if (result == MessageBoxResult.No)
            {
                DialogResult = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
        else
        {
            DialogResult = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete()
    {
        var result = HcMessageBox.Ask($"确定要删除 “{PathHelper.GetRelativePathOrOriginal(MappingFilePath)}”？", App.AppName);

        if (result == MessageBoxResult.OK)
        {
            File.Delete(MappingFilePath!);

            DialogResult = null;
            _confirmClosing = true;
            Messenger.Send(new ActionMessage(), "CloseFireTableColumnMappingWindow");
        }
    }

    [RelayCommand]
    private void DeleteSkipRowRule(FireTableSkipRowRule delete)
    {
        ColumnMapping.SkipRowRules.Remove(delete);
    }

    [RelayCommand]
    private void Loaded()
    {
        ColumnMapping.Validate();
        UpdateColumnOptions();
        ColumnMapping.AcceptChanges();
    }

    [RelayCommand]
    private void OpenDocument()
    {
        FileHelper.OpenFile(DocumentPath);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (String.IsNullOrWhiteSpace(MappingFilePath))
        {
            var dir = Directory.CreateDirectory("mappings");

            var dialog = new SaveFileDialog {
                DefaultDirectory = dir.FullName,
                InitialDirectory = dir.FullName,
                DefaultExt = ".firemap",
                Filter = "表列映射 (*.firemap)|*.firemap"
            };

            if (dialog.ShowDialog() is true)
            {
                MappingFilePath = dialog.FileName;
            }
            else
            {
                return;
            }
        }

        string json = JsonSerializer.Serialize(ColumnMapping, GlobalData.PrettyJsonOptions);
        await File.WriteAllTextAsync(MappingFilePath, json);

        DialogResult = true;
        _confirmClosing = true;
        Messenger.Send(new ActionMessage(), "CloseFireTableColumnMappingWindow");
    }

    #endregion Commands

    #region Methods

    public void UpdateColumnOptions()
    {
        int minRuleColumns = ColumnMapping.MinRuleColumns;

        // 如果最小规则列数大于当前的规则列数，则重置规则列数，要求用户重新选择
        if (minRuleColumns > ColumnMapping.RuleColumns)
        {
            ColumnMapping.RuleColumns = 0;
        }

        if (RuleColumnsNumbers.Length == 0 || minRuleColumns > ColumnMapping.RuleColumns)
        {
            RuleColumnsNumbers = [.. ComboBoxItemWrapper.CreateByValueDescription<FireTableColumnNumber>().Skip(minRuleColumns + 1)];
        }

        // 重新生成跳过规则的列数选项
        if (SkipRowRuleColumnsNumbers.Length != ColumnMapping.RuleColumns)
        {
            // 更新数据源将导致选择项 UI 被清空
            SkipRowRuleColumnsNumbers = [.. ComboBoxItemWrapper.CreateByValueDescription<FireTableColumnNumber>().Skip(2).Take(ColumnMapping.RuleColumns)];

            foreach (var rule in ColumnMapping.SkipRowRules)
            {
                if (rule.TargetColumn > ColumnMapping.RuleColumns)
                {
                    rule.TargetColumn = 0;
                }

                // 强制更新选择项 UI
                rule.RaiseTargetColumn();
            }
        }
    }

    #endregion Methods

    #region IDropTarget

    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is FireTableSkipRowRule && dropInfo.TargetItem is FireTableSkipRowRule)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }
    }

    public void Drop(IDropInfo dropInfo)
    {
        if (dropInfo.Data is FireTableSkipRowRule sourceItem)
        {
            int oldIndex = ColumnMapping.SkipRowRules.IndexOf(sourceItem);

            if (oldIndex == dropInfo.InsertIndex)
            {
                return;
            }

            int newIndex = dropInfo.InsertIndex > oldIndex ? dropInfo.InsertIndex - 1 : dropInfo.InsertIndex;

            ColumnMapping.SkipRowRules.Move(oldIndex, newIndex);
        }
    }

    #endregion IDropTarget
}
