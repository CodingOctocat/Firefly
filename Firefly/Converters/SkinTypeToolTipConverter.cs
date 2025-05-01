using System;
using System.Globalization;
using System.Windows.Data;

using HandyControl.Data;

namespace Firefly.Converters;

public class SkinTypeToolTipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var skinType = value as SkinType?;

        return skinType switch {
            SkinType.Default => $"主题: 浅色",
            SkinType.Dark => $"主题: 深色",
            SkinType.Violet => $"主题: 紫罗兰",
            _ => $"主题: 使用系统设置"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
