using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Firefly.Converters;

public class PrintableFontFamilyConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FontFamily fontFamily)
        {
            foreach (var typeface in fontFamily.GetTypefaces())
            {
                if (typeface.TryGetGlyphTypeface(out var glyphTypeface))
                {
                    if (glyphTypeface.Symbol)
                    {
                        return null;
                    }
                }
            }
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
