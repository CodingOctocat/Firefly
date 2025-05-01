using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Firefly.Helpers;
using Firefly.Properties;

using HandyControl.Data;

namespace Firefly.ViewModels;

public partial class ThemeSwitchButtonViewModel : ObservableObject
{
    #region Commands

    [RelayCommand]
    private static void ChangeTheme(SkinType? skin)
    {
        ThemeManager.UpdateSkin(skin);

        Settings.Default.Theme = ThemeManager.CurrentSkinTypeMode?.ToString();
    }

    [RelayCommand]
    private static void Loaded()
    {
        SkinType? skinType = null;

        if (Enum.TryParse(Settings.Default.Theme, out SkinType skin))
        {
            skinType = skin;
        }

        ChangeTheme(skinType);
    }

    #endregion Commands
}
