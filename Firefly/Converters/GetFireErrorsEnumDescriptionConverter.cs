using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using Firefly.Helpers;
using Firefly.Models;

namespace Firefly.Converters;

public class GetFireErrorsEnumDescriptionConverter : IValueConverter
{
    private static readonly Lazy<Dictionary<FireErrors, string>> _valueDescriptionDict = new(() =>
        Enum.GetValues<FireErrors>()
            .Zip(EnumHelper.GetDescriptions<FireErrors>(), (k, v) => new { k, v })
            .ToDictionary(x => x.k, x => x.v));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FireErrors fireErrors)
        {
            return _valueDescriptionDict.Value[fireErrors];
        }

        throw new ArgumentException($"期望类型为 {nameof(FireErrors)}，但收到 {value?.GetType().Name ?? "null"}", nameof(value));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
