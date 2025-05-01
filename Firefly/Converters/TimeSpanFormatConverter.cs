using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class TimeSpanFormatConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TimeSpan dt)
        {
            return null;
        }

        string format = parameter as string ?? "mm:ss";

        return format switch {
            "mm:ss" => $"{(int)dt.TotalMinutes:D2}:{dt.Seconds:D2}",
            "hh:mm:ss" => $"{(int)dt.TotalHours:D2}:{dt.Minutes:D2}:{dt.Seconds:D2}",
            _ => $"{(int)dt.TotalHours:D2}:{dt.Minutes:D2}:{dt.Seconds:D2}"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
