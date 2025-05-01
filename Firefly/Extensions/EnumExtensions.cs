using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Firefly.Extensions;

public static class EnumExtensions
{
    public static string GetDescription<TEnum>(this TEnum value) where TEnum : Enum
    {
        return value
            .GetType()
            .GetMember(value.ToString())
            .FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description
            ?? value.ToString();
    }

    public static string GetEnumMember<TEnum>(this TEnum value) where TEnum : Enum
    {
        return value
            .GetType()
            .GetMember(value.ToString())
            .FirstOrDefault()?.GetCustomAttribute<EnumMemberAttribute>()?.Value
            ?? value.ToString();
    }
}
