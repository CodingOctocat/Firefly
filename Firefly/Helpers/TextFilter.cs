using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Firefly.Helpers;

public static partial class TextFilter
{
    /// <summary>
    /// \u0020: Space,
    /// \u0009: Horizontal tab (HT),
    /// \u00A0: Non-break space,
    /// \u1680: Ogham space mark,
    /// \u180E: Mongolian vowel separator,
    /// \u2000: En quad,
    /// \u2001: Em quad,
    /// \u2002: En space,
    /// \u2003: Em space,
    /// \u2004: Three-per-em space,
    /// \u2005: Four-per-em space,
    /// \u2006: Six-per-em space,
    /// \u2007: Figure space,
    /// \u2008: Punctuation space,
    /// \u2009: Thin space,
    /// \u200A: Hair space,
    /// \u202F: Narrow no-break space,
    /// \u205F: Medium mathematical space,
    /// \u3000: Ideographic space.
    /// </summary>
    public static readonly string[] HSpaces = ["\u0020", "\t", "\u00A0", "\u1680", "\u180E", "\u2000", "\u2001", "\u2002", "\u2003", "\u2004", "\u2005", "\u2006", "\u2007", "\u2008", "\u2009", "\u200A", "\u202F", "\u205F", "\u3000"];

    /// <summary>
    /// \u000D\u000A: CRLF,
    /// \u000A: Linefeed (LF),
    /// \u000D: Carriage return (CR),
    /// \u000B: Vertical tab (VT),
    /// \u000C: Form feed (FF),
    /// \u0085: Next line (NEL),
    /// \u2028: Line separator (LS),
    /// \u2029: Paragraph separator (PS).
    /// </summary>
    public static readonly string[] VSpaces = ["\r\n", "\n", "\r", "\u000B", "\u000C", "\u0085", "\u2028", "\u2029"];

    /// <summary>
    /// 获取指定字符串的干净字符串（紧凑的）。
    /// 全角字符转半角字符，标准化空白（水平空白转化为英文空格，竖向空白转化为换行符（\n）），压缩空白（最大连续空白=1，无空行），修剪空白（逐行）。</summary>
    /// <param name="text"></param>
    /// <param name="h"></param>
    /// <param name="newH"></param>
    /// <param name="v"></param>
    /// <param name="newV"></param>
    /// <returns></returns>
    public static string CleanText(string text, bool h = false, string newH = "", bool v = false, string newV = "")
    {
        text = ToDBC(text);
        text = NormalizeWhiteSpace(text);

        text = MultiSpacesRegex().Replace(text, " ");
        text = MultiNewLinesRegex().Replace(text, "\n");

        var lines = text.Split('\n')
            .Where(x => !String.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim());

        text = String.Join('\n', lines);
        text = RemoveWhiteSpaces(text, h, newH, v, newV);

        return text;
    }

    public static bool ContainHSpaces(string text)
    {
        return VSpaces.Any(text.Contains);
    }

    public static bool ContainVSpaces(string text)
    {
        return HSpaces.Any(text.Contains);
    }

    /// <summary>
    /// 将指定字符串中的水平空白转化为英文空格，竖向空白转化为换行符（\n）。
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string NormalizeWhiteSpace(string text)
    {
        foreach (string hs in HSpaces)
        {
            text = text.Replace(hs, " ");
        }

        foreach (string vs in VSpaces)
        {
            text = text.Replace(vs, "\n");
        }

        return text;
    }

    /// <summary>
    /// 移除指定字符串中所有的空白。
    /// </summary>
    /// <param name="text"></param>
    /// <param name="h"></param>
    /// <param name="newH"></param>
    /// <param name="v"></param>
    /// <param name="newV"></param>
    /// <returns></returns>
    public static string RemoveWhiteSpaces(string text, bool h = true, string newH = "", bool v = true, string newV = "")
    {
        if (h)
        {
            foreach (string hs in HSpaces)
            {
                text = text.Replace(hs, newH);
            }
        }

        if (v)
        {
            foreach (string vs in VSpaces)
            {
                text = text.Replace(vs, newV);
            }
        }

        return text;
    }

    /// <summary>
    /// 全角字符转半角字符（DBC case）。
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToDBC(string input)
    {
        char[] c = input.ToCharArray();

        for (int i = 0; i < c.Length; i++)
        {
            if (c[i] == 12288)
            {
                // 全角空格为 12288，半角空格为 32
                c[i] = (char)32;

                continue;
            }

            if (c[i] is > (char)65280 and < (char)65375)
            {
                // 其他字符半角（33-126）与全角（65281-65374）的对应关系是：均相差 65248
                c[i] = (char)(c[i] - 65248);
            }
        }

        return new string(c);
    }

    #region Regex

    [GeneratedRegex("\n{2,}")]
    private static partial Regex MultiNewLinesRegex();

    [GeneratedRegex(" {2,}")]
    private static partial Regex MultiSpacesRegex();

    #endregion Regex
}
