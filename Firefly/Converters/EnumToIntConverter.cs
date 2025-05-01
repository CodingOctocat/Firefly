using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public partial class EnumToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var type = targetType;

        if (parameter is Type t)
        {
            type = t;
        }

        if (value is int intValue && type.IsEnum)
        {
            return Enum.ToObject(type, intValue);
        }

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return System.Convert.ToInt32(enumValue);
        }

        return Binding.DoNothing;
    }
}
