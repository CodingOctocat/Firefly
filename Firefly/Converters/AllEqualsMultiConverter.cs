using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class AllEqualsMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
        {
            return false;
        }

        object firstValue = values[0];

        foreach (object value in values)
        {
            if (!Equals(value, firstValue))
            {
                return false;
            }
        }

        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
