using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class SubtractOneConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue - 1;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue + 1;
        }

        return value;
    }
}
