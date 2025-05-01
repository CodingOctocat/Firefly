using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Firefly.Extensions;

namespace Firefly.Helpers;

public static class EnumHelper
{
    private static readonly ConcurrentDictionary<Type, Lazy<Dictionary<string, string>>> _descriptionEnumMemberCache = new();

    private static readonly ConcurrentDictionary<Type, Lazy<string[]>> _descriptionsCache = new();

    private static readonly ConcurrentDictionary<Type, Lazy<string[]>> _enumMembersCache = new();

    public static Dictionary<string, string> GetDescriptionEnumMemberDict<TEnum>() where TEnum : struct, Enum
    {
        return _descriptionEnumMemberCache.GetOrAdd(typeof(TEnum), _ => new Lazy<Dictionary<string, string>>(() => {
            return GetDescriptions<TEnum>().Zip(
                GetEnumMembers<TEnum>(),
                (k, v) => new { k, v })
            .ToDictionary(x => x.k, x => x.v);
        })).Value;
    }

    public static string[] GetDescriptions<TEnum>() where TEnum : struct, Enum
    {
        return _descriptionsCache.GetOrAdd(
            typeof(TEnum),
            _ => new Lazy<string[]>(static () => [.. Enum.GetValues<TEnum>().Select(x => x.GetDescription())])).Value;
    }

    public static string[] GetEnumMembers<TEnum>() where TEnum : struct, Enum
    {
        return _enumMembersCache.GetOrAdd(
            typeof(TEnum),
            _ => new Lazy<string[]>(static () => [.. Enum.GetValues<TEnum>().Select(x => x.GetEnumMember())])).Value;
    }
}
