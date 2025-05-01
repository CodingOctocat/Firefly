using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class EnumHasFlagConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue && parameter is Enum flagValue)
        {
            return enumValue.HasFlag(flagValue);
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
