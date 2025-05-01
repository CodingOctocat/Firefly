using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class FireTableCellInfoConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [string k, string v])
        {
            return new KeyValuePair<string, string>(k, v);
        }

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
