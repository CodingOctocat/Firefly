using System;
using System.Collections.Generic;
using System.Linq;

using Firefly.Helpers;

namespace Firefly.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// 获取指定字符串的干净字符串（紧凑的）。
    /// 全角字符转半角字符，标准化空白（水平空白转化为英文空格，竖向空白转化为换行符（\n）），压缩空白（最大连续空白=1，无空行），修剪空白（逐行）。</summary>
    /// <param name="text"></param>
    /// <param name="h"></param>
    /// <param name="newH"></param>
    /// <param name="v"></param>
    /// <param name="newV"></param>
    /// <returns></returns>
    public static string CleanText(this string text, bool h = false, string newH = "", bool v = false, string newV = "")
    {
        return TextFilter.CleanText(text, h, newH, v, newV);
    }

    public static bool ContainsAny(this string text, IEnumerable<string> values, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return MatchAnyFunc(text.Contains, values, comparison);
    }

    public static int EndIndexOfAny(this string text, IEnumerable<string> values, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        int lastIndex = -1;

        foreach (string value in values)
        {
            if (String.IsNullOrEmpty(value))
            {
                continue;
            }

            int index = text.LastIndexOf(value, comparison);

            if (index != -1)
            {
                index += value.Length - 1;
            }

            if (index > lastIndex)
            {
                lastIndex = index;
            }
        }

        return lastIndex;
    }

    public static bool EndsWithAny(this string text, IEnumerable<string> values, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return MatchAnyFunc(text.EndsWith, values, comparison);
    }

    public static string Format(this string format, params object[] args)
    {
        return String.Format(format, args);
    }

    public static string Remove(this string text, params string[] removedList)
    {
        foreach (string removed in removedList)
        {
            text = text.Replace(removed, "");
        }

        return text;
    }

    public static string RemovePrefix(this string text, string prefix)
    {
        if (text.StartsWith(prefix))
        {
            return text[prefix.Length..];
        }

        return text;
    }

    public static string RemoveSuffix(this string text, string suffix)
    {
        if (text.EndsWith(suffix))
        {
            return text[..^suffix.Length];
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
    public static string RemoveWhiteSpaces(this string text, bool h = true, string newH = "", bool v = true, string newV = "")
    {
        return TextFilter.RemoveWhiteSpaces(text, h, newH, v, newV);
    }

    public static bool StartsWithAnyOrdinal(this string text, IEnumerable<string> values, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return MatchAnyFunc(text.StartsWith, values, comparison);
    }

    /// <summary>
    /// 从当前字符串中删除 '/' 字符的所有前导和尾随实例。如果当前字符串仅由 '/' 字符组成，则返回 '/'。
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string TrimSlash(this string text)
    {
        if (text.All(c => c == '/'))
        {
            return "/";
        }

        text = text.Trim('/');

        return text;
    }

    private static bool MatchAnyFunc(Func<string, StringComparison, bool> func, IEnumerable<string> values, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        bool b = false;

        foreach (string v in values)
        {
            b |= func(v, comparison);

            if (b)
            {
                return b;
            }
        }

        return b;
    }
}
