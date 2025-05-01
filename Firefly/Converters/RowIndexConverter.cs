using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Firefly.Converters;

public class RowIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FrameworkElement element && ItemsControl.ItemsControlFromItemContainer(element) is ItemsControl itemsControl)
        {
            int index = itemsControl.ItemContainerGenerator.IndexFromContainer(element);

            return index + 1;
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
