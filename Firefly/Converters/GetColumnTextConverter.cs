using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

using Firefly.Helpers;

namespace Firefly.Converters;

public class GetColumnTextConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataGridCell cell)
        {
            // 查找 DataGridCell 内部的 TextBlock
            var textBlock = VisualTreeHelperEx.FindVisualChild<TextBlock>(cell);
            string? text = textBlock?.Text;

            return String.IsNullOrWhiteSpace(text) ? parameter : text;
        }

        return parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
