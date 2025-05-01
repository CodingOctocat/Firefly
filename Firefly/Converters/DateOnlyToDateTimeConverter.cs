using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class DateOnlyToDateTimeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        var dateOnly = (DateOnly)value;

        return new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        return DateOnly.FromDateTime((DateTime)value);
    }
}
