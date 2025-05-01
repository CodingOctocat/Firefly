using System;
using System.Collections.Generic;

namespace Firefly.Helpers;

/// <summary>
/// 基于 “Rolling Hash + 滑动窗口” 的子串匹配器。时间复杂度：O(L + N)
/// </summary>
public static class SubstringMatcher
{
    /// <summary>
    /// 判断目标文本 (text) 是否包含查询字符串 (query) 的任意一个长度为 2 的子串。
    /// </summary>
    /// <param name="text"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static bool IsSubstringPresent(string text, string query)
    {
        if (query.Length < 2 || String.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var querySubHashes = GetQueryHashes(query);

        for (int i = 0; i < text.Length - 1; i++)
        {
            string windowStr = text.Substring(i, 2);

            if (querySubHashes.Contains(GetHash(windowStr)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 计算字符串哈希值 (Rolling Hash 简化版)。
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static int GetHash(string str)
    {
        int hash = 0;

        foreach (char c in str)
        {
            hash = (hash * 31) + c;
        }

        return hash;
    }

    /// <summary>
    /// 计算 query 所有长度为 2 的子串的哈希值。
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    private static HashSet<int> GetQueryHashes(string query)
    {
        HashSet<int> querySubHashes = [];

        for (int i = 0; i < query.Length - 1; i++)
        {
            string subStr = query.Substring(i, 2);
            querySubHashes.Add(GetHash(subStr));
        }

        return querySubHashes;
    }
}
