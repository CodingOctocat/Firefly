using System.Linq;

using HandyControl.Controls;

namespace Firefly.Helpers;

public static class WindowHelper
{
    public static void CloseGrowlNotificationIfNecessary()
    {
        var allWindows = App.Current.Windows.OfType<Window>().ToList();

        var growlWindow = allWindows.FirstOrDefault(w => w is GrowlWindow);

        if (growlWindow is not null && allWindows.Count == 1)
        {
            growlWindow.Close();
        }
    }
}
