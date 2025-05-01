using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class NewLineToSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            return text.Replace("\r\n", "↵").Replace("\r", "↵").Replace("\n", "↵");
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
