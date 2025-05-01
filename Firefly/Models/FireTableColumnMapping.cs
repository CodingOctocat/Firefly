using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;

using Firefly.Common;

namespace Firefly.Models;

public partial class FireTableColumnMapping : ObservableObject, IChangeTracking
{
    #region 清单相对列号

    /// <summary>
    /// 「首列」（可用于序号）相对于当前行的列号。
    /// </summary>
    [JsonIgnore]
    public int C1Column { get; } = 1;

    /// <summary>
    /// 「证书编号」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(6)]
    public partial int CertificateNumberColumn { get; set; }

    /// <summary>
    /// 「数量」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(3)]
    public partial int CountColumn { get; set; }

    [ObservableProperty]
    [JsonPropertyOrder(0)]
    public partial string Description { get; set; } = "";

    /// <summary>
    /// 「生产厂家」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(5)]
    public partial int EnterpriseNameColumn { get; set; }

    /// <summary>
    /// 「消防系统」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFireSystemFullRow))]
    [JsonPropertyOrder(1)]
    public partial int FireSystemColumn { get; set; }

    /// <summary>
    /// 「出厂日期」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(9)]
    public partial int ManufactureDateColumn { get; set; }

    /// <summary>
    /// 「产品型号」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(4)]
    public partial int ModelColumn { get; set; }

    /// <summary>
    /// 「产品名称」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(2)]
    public partial int ProductNameColumn { get; set; }

    /// <summary>
    /// 「备注」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(10)]
    public partial int RemarkColumn { get; set; }

    /// <summary>
    /// 「检验报告」列相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(7)]
    public partial int ReportNumberColumn { get; set; }

    /// <summary>
    /// 「合格证」列（可用于打勾或填写证书状态）相对于当前行的列号。
    /// </summary>
    [ObservableProperty]
    [JsonPropertyOrder(8)]
    public partial int StatusColumn { get; set; }

    #endregion 清单相对列号

    public bool IsCertificateAndReportNumberInSameColumn => CertificateNumberColumn == ReportNumberColumn;

    [ObservableProperty]
    [JsonIgnore]
    public partial bool IsChanged { get; private set; }

    public bool? IsFireSystemFullRow => FireSystemColumn switch {
        -1 => true,
        0 => null,
        _ => false
    };

    public bool IsModelAndEnterpriseNameInSameColumn => ModelColumn == EnterpriseNameColumn;

    public int MinRuleColumns => new int[] { FireSystemColumn, ProductNameColumn, CountColumn, ModelColumn, EnterpriseNameColumn, CertificateNumberColumn, ReportNumberColumn, StatusColumn, ManufactureDateColumn, RemarkColumn }.Max();

    [ObservableProperty]
    [JsonPropertyOrder(11)]
    public partial int RuleColumns { get; set; }

    [JsonPropertyOrder(12)]
    [JsonInclude]
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public ObservableRangeCollection<FireTableSkipRowRule> SkipRowRules { get; [Obsolete("仅用于支持 JSON 序列化。")] private set; } = [];

    public bool SkipRowRulesHasError => SkipRowRules.Any(r => r.HasError);

    [JsonConstructor]
    public FireTableColumnMapping()
    {
        SkipRowRules.CollectionChanged += SkipRowRules_CollectionChanged;
    }

    public void AcceptChanges()
    {
        IsChanged = false;
    }

    public override string ToString()
    {
        return $"描述: {(String.IsNullOrWhiteSpace(Description) ? "无" : Description)}\n跳过规则: {SkipRowRules.Count} 条";
    }

    #region 错误检查

    [JsonIgnore]
    public Dictionary<string, bool> Errors { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial bool HasConflictingColumn { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial bool HasEmptyRequiredColumn { get; private set; }

    public bool HasError => HasEmptyRequiredColumn || HasConflictingColumn || SkipRowRulesHasError;

    public bool GetError(string property)
    {
        return Errors.TryGetValue(property, out bool error) && error;
    }

    public bool Validate()
    {
        var emptyRequiredColumns = new List<string>();

        if (ProductNameColumn == 0)
        {
            emptyRequiredColumns.Add(nameof(ProductNameColumn));
        }

        if (ModelColumn == 0)
        {
            emptyRequiredColumns.Add(nameof(ModelColumn));
        }

        if (EnterpriseNameColumn == 0)
        {
            emptyRequiredColumns.Add(nameof(EnterpriseNameColumn));
        }

        if (CertificateNumberColumn == 0)
        {
            emptyRequiredColumns.Add(nameof(CertificateNumberColumn));
        }

        if (RuleColumns == 0)
        {
            emptyRequiredColumns.Add(nameof(RuleColumns));
            SetError(nameof(RuleColumns), true);
        }
        else
        {
            SetError(nameof(RuleColumns), false);
        }

        var selectedValues = new Dictionary<string, int> {
            [nameof(FireSystemColumn)] = FireSystemColumn,
            [nameof(ProductNameColumn)] = ProductNameColumn,
            [nameof(CountColumn)] = CountColumn,
            [nameof(ModelColumn)] = ModelColumn,
            [nameof(EnterpriseNameColumn)] = EnterpriseNameColumn,
            [nameof(CertificateNumberColumn)] = CertificateNumberColumn,
            [nameof(ReportNumberColumn)] = ReportNumberColumn,
            [nameof(StatusColumn)] = StatusColumn,
            [nameof(ManufactureDateColumn)] = ManufactureDateColumn,
            [nameof(RemarkColumn)] = RemarkColumn
        };

        // 找出有重复值（非 0）的列
        var conflictingColumns = selectedValues
            .Where(x => x.Value != 0)
            .GroupBy(x => x.Value)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Select(x => x.Key))
            .ToHashSet();

        // 定义方法来尝试移除特定的重复值对
        void TryRemoveDuplicatePair(string key1, string key2)
        {
            if (conflictingColumns.Contains(key1) && conflictingColumns.Contains(key2)
                && selectedValues[key1] == selectedValues[key2]
                && selectedValues.Where(x => x.Key != key1 && x.Key != key2).All(x => x.Value != selectedValues[key1]))
            {
                conflictingColumns.Remove(key1);
                conflictingColumns.Remove(key2);
            }
        }

        // 处理 ModelColumn 和 EnterpriseNameColumn
        TryRemoveDuplicatePair(nameof(ModelColumn), nameof(EnterpriseNameColumn));

        // 处理 CertificateNumberColumn 和 ReportNumberColumn
        TryRemoveDuplicatePair(nameof(CertificateNumberColumn), nameof(ReportNumberColumn));

        HasEmptyRequiredColumn = emptyRequiredColumns.Count != 0;
        HasConflictingColumn = conflictingColumns.Count != 0;

        foreach (string property in selectedValues.Keys)
        {
            SetError(property, emptyRequiredColumns.Contains(property) || conflictingColumns.Contains(property));
        }

        OnPropertyChanged(nameof(HasError));

        return !HasError;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName
            is nameof(FireSystemColumn)
            or nameof(ProductNameColumn)
            or nameof(CountColumn)
            or nameof(ModelColumn)
            or nameof(EnterpriseNameColumn)
            or nameof(CertificateNumberColumn)
            or nameof(ReportNumberColumn)
            or nameof(StatusColumn)
            or nameof(ManufactureDateColumn)
            or nameof(RemarkColumn)
            or nameof(RuleColumns))
        {
            Validate();
        }
        else if (e.PropertyName != nameof(IsChanged))
        {
            IsChanged = true;
        }

        base.OnPropertyChanged(e);
    }

    private void SetError(string property, bool hasError)
    {
        if (Errors.TryGetValue(property, out bool existing) && existing == hasError)
        {
            return;
        }

        Errors[property] = hasError;
        // 仅用于通知多重数据绑定
        OnPropertyChanged(nameof(Errors));
    }

    private void SkipRowRule_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FireTableSkipRowRule.HasError))
        {
            OnPropertyChanged(nameof(SkipRowRulesHasError));
            OnPropertyChanged(nameof(HasError));
        }

        IsChanged = true;
    }

    private void SkipRowRules_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (FireTableSkipRowRule item in e.OldItems)
            {
                item.PropertyChanged -= SkipRowRule_PropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (FireTableSkipRowRule item in e.NewItems)
            {
                item.PropertyChanged += SkipRowRule_PropertyChanged;
            }
        }

        OnPropertyChanged(nameof(SkipRowRulesHasError));
        OnPropertyChanged(nameof(HasError));

        IsChanged = true;
    }

    #endregion 错误检查
}
