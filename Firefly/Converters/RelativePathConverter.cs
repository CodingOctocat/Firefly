using System;
using System.Globalization;
using System.Windows.Data;

using Firefly.Helpers;

namespace Firefly.Converters;

public class RelativePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            return PathHelper.GetRelativePathOrOriginal(path);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
