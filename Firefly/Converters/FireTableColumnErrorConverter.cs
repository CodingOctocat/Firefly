using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class FireTableColumnErrorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is Dictionary<string, bool> errors && values[1] is string property)
        {
            return errors.TryGetValue(property, out bool error) && error;
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
