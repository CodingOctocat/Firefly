using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NPOI.OpenXmlFormats.Wordprocessing;

using NPOI.XWPF.UserModel;

namespace Firefly.Helpers;

/// <summary>
/// 提供 NPOI 操作 Word 文档的静态方法。
/// </summary>
public static class DocxHelper
{
    public static void AddOrderList(XWPFDocument doc, IEnumerable<XWPFTableCell> cells)
    {
        var numbering = doc.CreateNumbering();

        var cT_AbstractNum = new CT_AbstractNum {
            abstractNumId = "1",
            lvl = [
                new CT_Lvl() {
                    numFmt = new CT_NumFmt() { val = ST_NumberFormat.@decimal },
                    lvlText = new CT_LevelText() { val = "%1" },
                    start = new CT_DecimalNumber() { val = "1" }
                }]
        };

        string abstractNumId = numbering.AddAbstractNum(new XWPFAbstractNum(cT_AbstractNum));
        string numId = numbering.AddNum(abstractNumId);

        foreach (var cell in cells)
        {
            UnVMergedCell(cell);
            RemoveAllParagraphs(cell);

            // 拆分合并单元格后，会缺少边框，需要补充
            var ctTc = cell.GetCTTc();
            var tcPr = ctTc.tcPr ?? ctTc.AddNewTcPr();
            var border = tcPr.tcBorders ?? tcPr.AddNewTcBorders();
            border.top = new CT_Border() { val = ST_Border.single };
            border.bottom = new CT_Border() { val = ST_Border.single };

            cell.Paragraphs[0].SetNumID(numId);
        }
    }

    /// <summary>
    /// 将一个字符串追加到单元格末尾，如果字符串包含换行符，则处理成空格。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="text"></param>
    /// <param name="fontFamily"></param>
    /// <param name="fontSize"></param>
    /// <param name="fg"></param>
    /// <param name="bg"></param>
    /// <param name="isBold"></param>
    /// <param name="isStrikeThrough"></param>
    /// <param name="alignment"></param>
    /// <param name="verticalAlignment"></param>
    /// <param name="useBlankLine"></param>
    /// <returns></returns>
    public static XWPFParagraph AppendCellTextLine(XWPFTableCell cell, string text, string? fontFamily = null, double fontSize = -1, string fg = "auto", string bg = "auto",
        bool isBold = false, bool isStrikeThrough = false,
        ParagraphAlignment alignment = ParagraphAlignment.CENTER, TextAlignment verticalAlignment = TextAlignment.CENTER, bool useBlankLine = true)
    {
        static XWPFParagraph FormatParagraph(XWPFParagraph p, string bg = "auto",
            ParagraphAlignment alignment = ParagraphAlignment.CENTER, TextAlignment verticalAlignment = TextAlignment.CENTER)
        {
            p.FillBackgroundColor = bg;
            p.Alignment = alignment;
            p.VerticalAlignment = verticalAlignment;

            return p;
        }

        if (useBlankLine && cell.Paragraphs[^1].ParagraphText == "")
        {
            cell.Paragraphs[^1] = FormatParagraph(cell.Paragraphs[^1], bg, alignment, verticalAlignment);

            XWPFRun run;

            if (cell.Paragraphs[^1].Runs.Count == 0)
            {
                run = cell.Paragraphs[^1].CreateRun();
            }
            else
            {
                run = cell.Paragraphs[^1].Runs[^1];
            }

            fontFamily ??= cell.Paragraphs[^1].Runs[^1].FontFamily;

            if (fontSize < 0)
            {
                fontSize = cell.Paragraphs[^1].Runs[^1].FontSize;
            }

            FormatRun(run, text, fontFamily, fontSize, fg, isBold, isStrikeThrough);

            return cell.Paragraphs[^1];
        }

        var p = cell.AddParagraph();
        p = FormatParagraph(p);

        var r0 = p.CreateRun();
        FormatRun(r0, text, fontFamily, fontSize, fg, isBold, isStrikeThrough);

        return p;
    }

    /// <summary>
    /// 判断一个单元格是否包含超链接。
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool ContainHyperlink(XWPFTableCell cell)
    {
        var ps = cell.GetCTTc().GetPList();

        foreach (var p in ps)
        {
            if (p.GetHyperlinkList().Any())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 填充单元格的背景颜色。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="color"></param>
    public static void FillColor(XWPFTableCell cell, string color)
    {
        cell.SetColor(color);
    }

    /// <summary>
    /// 格式化文本块。
    /// </summary>
    /// <param name="run"></param>
    /// <param name="text"></param>
    /// <param name="fontFamily"></param>
    /// <param name="fontSize"></param>
    /// <param name="fg"></param>
    /// <param name="isBold"></param>
    /// <param name="isStrikeThrough"></param>
    /// <returns></returns>
    public static XWPFRun FormatRun(XWPFRun run, string text, string? fontFamily = null, double fontSize = -1, string fg = "auto", bool isBold = false, bool isStrikeThrough = false)
    {
        run.SetText(text);

        if (fontFamily is not null)
        {
            run.SetFontFamily(fontFamily, FontCharRange.None);
        }

        if (fontSize >= 0)
        {
            run.FontSize = fontSize;
        }

        run.SetColor(fg);
        run.IsBold = isBold;
        run.IsStrikeThrough = isStrikeThrough;

        return run;
    }

    /// <summary>
    /// 获取文档中指定位置的单元格。
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="tableIndex"></param>
    /// <param name="rowIndex"></param>
    /// <param name="cellIndex"></param>
    /// <returns></returns>
    public static XWPFTableCell GetCell(XWPFDocument doc, int tableIndex, int rowIndex, int cellIndex)
    {
        return GetTables(doc)[tableIndex].Rows[rowIndex].GetCell(cellIndex);
    }

    /// <summary>
    /// 获取指定单元格中的干净文本。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="selector"></param>
    /// <param name="h"></param>
    /// <param name="newH"></param>
    /// <param name="v"></param>
    /// <param name="newV"></param>
    /// <returns></returns>
    public static string? GetCellCleanText(XWPFTableCell cell, Func<string, string?>? selector, bool h = false, string newH = "", bool v = false, string newV = "")
    {
        if (selector is null)
        {
            return GetCellTextAs(cell, x => TextFilter.CleanText(x, h, newH, v, newV));
        }

        return GetCellTextAs(cell, x => selector(TextFilter.CleanText(x, h, newH, v, newV)));
    }

    /// <summary>
    /// 获取指定单元格中的干净文本。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="h"></param>
    /// <param name="newH"></param>
    /// <param name="v"></param>
    /// <param name="newV"></param>
    /// <returns></returns>
    public static string GetCellCleanText(XWPFTableCell cell, bool h = false, string newH = "", bool v = false, string newV = "")
    {
        return GetCellTextAs(cell, x => TextFilter.CleanText(x, h, newH, v, newV))!;
    }

    /// <summary>
    /// 获取指定单元格中的文本。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static string GetCellText(XWPFTableCell cell, Func<string, string>? selector = default)
    {
        return GetCellTextAs(cell, selector ??= _ => _) ?? "";
    }

    /// <summary>
    /// 获取指定单元格的内容。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cell"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    [return: MaybeNull]
    public static T GetCellTextAs<T>(XWPFTableCell cell, Func<string, T> selector)
    {
        // 使用 cell.GetText() 则不保留换行符
        var texts = from p in cell.Paragraphs
                    select p.ParagraphText;

        string text = String.Join(Environment.NewLine, texts);

        var data = selector(text);

        return data;
    }

    /// <summary>
    /// 获取文档中指定位置的表格行。
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="tableIndex"></param>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    public static XWPFTableRow GetRow(XWPFDocument doc, int tableIndex, int rowIndex)
    {
        return GetTables(doc)[tableIndex].Rows[rowIndex];
    }

    /// <summary>
    /// 获取文档中指定位置的表格。
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static XWPFTable GetTable(XWPFDocument doc, int index)
    {
        return GetTables(doc)[index];
    }

    /// <summary>
    /// 获取表格中的列数。
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public static int GetTableColumnCount(XWPFTable table)
    {
        return table.Rows.Max(r => r.GetTableCells().Count);
    }

    /// <summary>
    /// 获取指定文档中的所有表格。
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static IList<XWPFTable> GetTables(XWPFDocument doc)
    {
        return doc.Tables;
    }

    /// <summary>
    /// 获取表格行中指定位置的垂直合并单元格中的文本。
    /// </summary>
    /// <param name="row"></param>
    /// <param name="colIndex"></param>
    /// <param name="clean"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static string? GetVMergeCellText(XWPFTableRow row, int colIndex, bool clean = true, Func<string?, string?>? selector = default)
    {
        var table = row.GetTable();
        int rowIndex = table.Rows.IndexOf(row);

        // 如果存在多个匹配项，则可能无法得到预期的结果
        var restart = table.Rows
            .Take(rowIndex + 1)
            .Select(r => r.GetTableCells().ElementAtOrDefault(colIndex))
            .LastOrDefault(IsVMergeRestartCell);

        if (restart is null)
        {
            return null;
        }
        else
        {
            return clean ? GetCellCleanText(restart, selector) : GetCellText(restart, selector!);
        }
    }

    /// <summary>
    /// 判断文档是否插入了文字环绕型图片。对于非嵌入型图片，NPOI 生成的文档可能无法被 Microsoft Word 等文字处理软件打开。
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static bool HasAnchorPictures(XWPFDocument doc)
    {
        var ps = doc.Paragraphs;

        foreach (var p in ps)
        {
            var rs = p.Runs;

            foreach (var r in rs)
            {
                var ds = r.GetCTR().GetDrawingList();

                foreach (var d in ds)
                {
                    if (d.anchor.Count > 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 判断指定单元格是否为垂直合并单元格。
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool IsVMergeCell(XWPFTableCell cell)
    {
        return IsVMergeRestartCell(cell) || IsVMergeContinueCell(cell);
    }

    /// <summary>
    /// 判断指定单元格是否为被垂直合并的单元格。
    /// <para>垂直合并单元格中的除了第一个单元格的其他单元格。</para>
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool IsVMergeContinueCell(XWPFTableCell cell)
    {
        var vMerge = cell?.GetCTTc()?.tcPr?.vMerge;

        return vMerge is not null && vMerge.val == ST_Merge.@continue;
    }

    /// <summary>
    /// 判断指定单元格是否为要垂直合并的单元格。
    /// <para>垂直合并单元格中的第一个单元格。</para>
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool IsVMergeRestartCell(XWPFTableCell? cell)
    {
        var vMerge = cell?.GetCTTc()?.tcPr?.vMerge;

        return vMerge is not null && vMerge.val == ST_Merge.restart;
    }

    /// <summary>
    /// 从文件流中加载一个 <seealso cref="XWPFDocument"/> 对象，这是一个耗时操作。
    /// </summary>
    /// <param name="fs"></param>
    /// <returns></returns>
    public static async Task<XWPFDocument> LoadDocumentAsync(FileStream fs)
    {
        return await Task.Run(() => {
            var doc = new XWPFDocument(fs);

            return doc;
        });
    }

    /// <summary>
    /// 从路径中加载一个 <seealso cref="XWPFDocument"/> 对象，这是一个耗时操作。
    /// </summary>
    /// <param name="path"></param>
    /// <param name="mode"></param>
    /// <param name="access"></param>
    /// <param name="share"></param>
    /// <returns></returns>
    public static async Task<XWPFDocument> LoadDocumentAsync(string path, FileMode mode, FileAccess access, FileShare share)
    {
        return await Task.Run(() => {
            using var fs = new FileStream(path, mode, access, share);
            var doc = new XWPFDocument(fs);

            return doc;
        });
    }

    /// <summary>
    /// 重新格式化一个指定的单元格中的某段文本。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="search"></param>
    /// <param name="formatTexts"></param>
    /// <returns></returns>
    public static XWPFParagraph? Reformat(XWPFTableCell cell, Func<XWPFParagraph, bool> search,
        params (string text, string fontFamily, double fontSize, string fg, bool isBold, bool isStrikeThrough)[] formatTexts)
    {
        var p = cell.Paragraphs.FirstOrDefault(search);

        if (p is null)
        {
            return null;
        }

        while (p.Runs.Count > 1)
        {
            p.RemoveRun(0);
        }

        FormatRun(p.Runs[0], formatTexts[0].text, formatTexts[0].fontFamily, formatTexts[0].fontSize, formatTexts[0].fg, formatTexts[0].isBold, formatTexts[0].isStrikeThrough);

        if (formatTexts.Length > 1)
        {
            foreach ((string text, string fontFamily, double fontSize, string fg, bool isBold, bool isStrikeThrough) in formatTexts[1..])
            {
                var run = p.CreateRun();
                FormatRun(run, text, fontFamily, fontSize, fg, isBold, isStrikeThrough);
            }
        }

        return p;
    }

    /// <summary>
    /// 重新格式化指定单元格中的所有文本块。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="fontFamily"></param>
    /// <param name="fontSize"></param>
    public static void ReformatAllRuns(XWPFTableCell cell, string fontFamily, double fontSize)
    {
        if (cell.Paragraphs.Count == 1 && cell.Paragraphs[0].Runs?.Count == 0)
        {
            cell.Paragraphs[0].CreateRun();
        }

        foreach (var p in cell.Paragraphs)
        {
            foreach (var run in p.Runs)
            {
                run.SetFontFamily(fontFamily, FontCharRange.None);
                run.FontSize = fontSize;
            }
        }
    }

    /// <summary>
    /// 移除指定单元格中的所有满足要求的段落。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="match"></param>
    public static void RemoveAllParagraphs(XWPFTableCell cell, Predicate<string>? match = default)
    {
        match ??= _ => true;

        var removes = new List<int>();

        for (int i = cell.Paragraphs.Count - 1; i >= 0; i--)
        {
            if (match(cell.Paragraphs[i].ParagraphText))
            {
                removes.Add(i);
            }
        }

        for (int i = 0; i < removes.Count; i++)
        {
            cell.RemoveParagraph(removes[i]);
        }

        if (cell.Paragraphs.Count == 0)
        {
            cell.AddParagraph();
        }
    }

    /// <summary>
    /// 移除指定单元格中的空白行。
    /// </summary>
    /// <param name="cell"></param>
    public static void RemoveBlankParagraphs(XWPFTableCell cell)
    {
        RemoveAllParagraphs(cell, String.IsNullOrWhiteSpace);
    }

    /// <summary>
    /// 替换指定单元格中的文本。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public static void ReplaceCellText(XWPFTableCell cell, string oldValue, string newValue)
    {
        var ps = cell.Paragraphs;

        foreach (var p in ps)
        {
            string old = p.ParagraphText;

            if (String.IsNullOrWhiteSpace(old))
            {
                continue;
            }

            string @new = p.ParagraphText;

            if (old.Contains(oldValue))
            {
                @new = @new.Replace(oldValue, newValue);
            }

            p.ReplaceText(old, @new);
        }

        RemoveBlankParagraphs(cell);
    }

    /// <summary>
    /// 设置指定单元格的对齐方式。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="alignment"></param>
    /// <param name="verticalAlignment"></param>
    public static void SetCellAlignment(XWPFTableCell cell, ParagraphAlignment alignment = ParagraphAlignment.CENTER, TextAlignment verticalAlignment = TextAlignment.CENTER)
    {
        foreach (var p in cell.Paragraphs)
        {
            p.Alignment = alignment;
            p.VerticalAlignment = verticalAlignment;
        }
    }

    /// <summary>
    /// 设置指定单元格中的文本块为粗体。
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="isBold"></param>
    public static void SetCellBold(XWPFTableCell cell, bool isBold = true)
    {
        foreach (var p in cell.Paragraphs)
        {
            foreach (var r in p.Runs)
            {
                r.IsBold = isBold;
            }
        }
    }

    /// <summary>
    /// 尝试拆分指定的单元格。
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static bool UnVMergedCell(XWPFTableCell cell)
    {
        try
        {
            var cttc = cell.GetCTTc();
            cttc.tcPr.vMerge = null;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task WriteAsync(XWPFDocument doc, string path)
    {
        await using var fs = File.Create(path);
        await Task.Run(() => doc.Write(fs));
    }
}
