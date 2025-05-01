using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class StringContainsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 3 && values[0] is bool enabled && values[1] is string text && values[2] is string findText)
        {
            if (!enabled)
            {
                return false;
            }

            if (String.IsNullOrWhiteSpace(text) || String.IsNullOrWhiteSpace(findText))
            {
                return false;
            }

            bool b = text.Contains(findText, StringComparison.OrdinalIgnoreCase);

            return b;
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
