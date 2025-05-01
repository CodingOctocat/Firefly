using HandyControl.Data;

using static Firefly.Helpers.ThemeWatcher;

namespace Firefly.Extensions;

public static class WindowsThemeExtensions
{
    public static SkinType ToSkinType(this WindowsTheme windowsTheme)
    {
        return windowsTheme switch {
            WindowsTheme.Dark => SkinType.Dark,
            _ => SkinType.Default
        };
    }
}
