using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class NullableBoolToBoolConverter : IValueConverter
{
    public bool Invert { get; set; } = false;

    public bool NullAs { get; set; } = false;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool? b = value as bool?;

        bool result = b ?? NullAs;

        return Invert ? !result : result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool? b = value as bool?;

        bool result = b.GetValueOrDefault(NullAs);

        return Invert ? !result : result;
    }
}
