using System;
using System.Globalization;
using System.Windows.Data;

namespace Firefly.Converters;

public class StringToBoolConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text && !String.IsNullOrWhiteSpace(text))
        {
            return !Invert;
        }

        return Invert;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
