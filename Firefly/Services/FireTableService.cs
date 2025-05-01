using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Firefly.Common;
using Firefly.Factories;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Services.Abstractions;

using NPOI.XWPF.UserModel;

namespace Firefly.Services;

public partial class FireTableService : ObservableObject, IDisposable
{
    private readonly IDocxProvider _docxProvider;

    private readonly IFireCheckContextFactory _fireCheckContextFactory;

    private readonly IFireTableReader _fireTableReader;

    private readonly IFireTableWriter _fireTableWriter;

    public XWPFDocument? Document { get; private set; }

    [ObservableProperty]
    public partial int ErrorItemsCount { get; private set; }

    public ObservableRangeCollection<FireCheckContext> FireCheckContexts { get; } = [];

    public FireTableService(IDocxProvider docxProvider, IFireTableReader fireTableReader, IFireCheckContextFactory fireCheckContextFactory, IFireTableWriter fireTableWriter)
    {
        _docxProvider = docxProvider;
        _fireTableReader = fireTableReader;
        _fireCheckContextFactory = fireCheckContextFactory;
        _fireTableWriter = fireTableWriter;
    }

    public async Task CheckAsync(int delay = 500, IProgress<int>? progress = default, CancellationToken cancellationToken = default)
    {
        ErrorItemsCount = 0;
        int i = 1;

        foreach (var ctx in FireCheckContexts)
        {
            progress?.Report(i++);
            await ctx.CheckCommand.ExecuteAsync(cancellationToken);

            if (ctx.HasError is true)
            {
                ErrorItemsCount++;
            }

            await Task.Delay(Jitter.Next(delay, 0.5), cancellationToken);
        }
    }

    /// <summary>
    /// 判断是否为预设表列映射。
    /// <para>「福建消防技术服务信息平台 > 建筑消防设施检测报告 > 检查项目概况附表」</para>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool IsPresetFireTable()
    {
        if (Document is null)
        {
            throw new InvalidOperationException("Document is not loaded.");
        }

        try
        {
            // 检查表格是否刚好 8 列（由于 NPOI 不支持获取 Word 表格的实际列数，所以通过第 4 行的单元格数量来判断）
            bool isTable8ColumnCount = DocxHelper.GetRow(Document, 0, 3).GetTableCells().Count == 8;
            // 检查第 1 行是否只有 1 列
            bool isFirstRowOnlyOneColumn = DocxHelper.GetRow(Document, 0, 0).GetTableCells().Count == 1;
            // 检查第 2 行是否刚好 5 列（设备名称 | 数量 | 产品型号/生产厂家 | 主要消费产品证书 | 备注）
            bool isSecondRow5Column = DocxHelper.GetRow(Document, 0, 1).GetTableCells().Count == 5;
            // 检查第 2 行第 1 列是否是合并单元格（设备名称）
            bool isR2C1IsMergedCell = DocxHelper.IsVMergeCell(DocxHelper.GetCell(Document, 0, 1, 0));
            // 检查第 3 行是否刚好 7 列（设备名称 | 数量 | 产品型号/生产厂家 | 符合法定市场准入规则的证明文件 | 合格证 | 出厂日期 | 备注）
            bool isThirdRow7Column = DocxHelper.GetRow(Document, 0, 2).GetTableCells().Count == 7;

            bool[] conditions = [isTable8ColumnCount, isFirstRowOnlyOneColumn, isR2C1IsMergedCell, isSecondRow5Column, isThirdRow7Column];

            return conditions.Count(x => x) > 2;
        }
        catch
        {
            return false;
        }
    }

    public async Task ReadAsync(string filePath, FireTableColumnMapping columnMapping)
    {
        Document?.Dispose();
        Document = await _docxProvider.LoadAsync(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        FireCheckContext.ResetOrder();
        var fireProducts = _fireTableReader.Read(Document, columnMapping);
        FireCheckContexts.ReplaceRange(fireProducts.Select(_fireCheckContextFactory.Create));
    }

    public async Task WriteAsync(string filePath)
    {
        if (Document is null)
        {
            throw new InvalidOperationException("Document is not loaded.");
        }

        await _fireTableWriter.WriteAsync(filePath, Document, FireCheckContexts);
    }

    #region IDisposable

    public void Dispose()
    {
        Document?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}
