using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class EnumEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue && parameter is Enum target)
        {
            return enumValue.Equals(target);
        }

        return false;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter is Enum target)
        {
            return target;
        }

        return null;
    }
}
