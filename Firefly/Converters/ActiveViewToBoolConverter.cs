using System;
using System.Globalization;
using System.Windows.Data;

using Firefly.Models;

namespace Firefly.Converters;

public class ActiveViewToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ActiveView view)
        {
            return view != ActiveView.Firefly;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? ActiveView.CccfQuery : ActiveView.Firefly;
        }

        return ActiveView.Firefly;
    }
}
