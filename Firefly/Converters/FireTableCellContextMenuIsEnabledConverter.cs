using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class FireTableCellContextMenuIsEnabledConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (String.IsNullOrWhiteSpace(text) || text is "/" or "N/A")
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
