using System;
using System.Linq;
using System.Text.RegularExpressions;

using Firefly.Extensions;
using Firefly.Helpers;

using NPOI.XWPF.UserModel;

namespace Firefly.Models;

public class FireCell
{
    private static readonly string[] _fireErrorDescriptions = EnumHelper.GetDescriptions<FireErrors>();

    public XWPFTableCell Cell { get; }

    public string Raw { get; }

    public string Text { get; }

    public FireCell(XWPFTableCell cell)
    {
        Cell = cell;
        Raw = DocxHelper.GetCellText(cell).Trim();
        string text = DocxHelper.GetCellText(cell) ?? "";

        string pattern = String.Join("|", _fireErrorDescriptions.Select(Regex.Escape));
        Text = Regex.Replace(text, pattern, "").CleanText();
    }

    public FireCell(XWPFTableCell cell, string raw)
    {
        Cell = cell;
        Raw = raw.Trim();

        string pattern = String.Join("|", _fireErrorDescriptions.Select(Regex.Escape));
        string cleanText = Regex.Replace(raw, pattern, "").CleanText();

        if (cleanText == "")
        {
            Text = "";
        }
        else
        {
            Text = cleanText.TrimSlash();
        }
    }

    public override string ToString()
    {
        return $"[{Raw}]=>[{Text}]";
    }
}
