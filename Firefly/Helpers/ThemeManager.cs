using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using Firefly.Extensions;

using HandyControl.Data;

namespace Firefly.Helpers;

public static class ThemeManager
{
    public static SkinType CurrentSkinType
    {
        get;
        private set
        {
            field = value;
            OnStaticPropertyChanged();
        }
    }

    /// <summary>
    /// <para><see cref="SkinType.Default"/>: Light</para>
    /// <para><see cref="SkinType.Dark"/>: Dark</para>
    /// <para><see cref="SkinType.Violet"/>: Violet</para>
    /// <para><see langword="null"/>: Use System Setting</para>
    /// </summary>
    public static SkinType? CurrentSkinTypeMode
    {
        get;
        private set
        {
            field = value;
            OnStaticPropertyChanged();
        }
    }

    public static bool IsThemeChanging { get; private set; }

    public static event EventHandler<SkinType?>? ThemeChanged;

    public static event EventHandler<SkinType?>? ThemeChanging;

    public static void UpdateSkin(SkinType? skin)
    {
        SkinType actualSkinType;

        if (skin is null)
        {
            actualSkinType = ThemeWatcher.GetCurrentWindowsTheme().ToSkinType();
        }
        else
        {
            actualSkinType = skin.Value;
        }

        if (actualSkinType == CurrentSkinType)
        {
            CurrentSkinTypeMode = skin;

            return;
        }

        IsThemeChanging = true;
        ThemeChanging?.Invoke(null, skin);

        App.Current.Resources.MergedDictionaries.Clear();

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new($"pack://application:,,,/HandyControl;component/Themes/Skin{actualSkinType}.xaml")
        });

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
        });

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new("/Styles/FixHandyControlTextBoxPlusTemplate.xaml", UriKind.Relative)
        });

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new("/Styles/FixHandyControlSearchBarPlusTemplate.xaml", UriKind.Relative)
        });

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new("/Styles/FixHandyControlComboBoxEditableLeftTemplate.xaml", UriKind.Relative)
        });

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new("/Styles/MyResources.xaml", UriKind.Relative)
        });

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new("/Styles/MyStyles.xaml", UriKind.Relative)
        });

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new("/Styles/CccfStyles.xaml", UriKind.Relative)
        });

        App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new("/Styles/FireTableStyles.xaml", UriKind.Relative)
        });

        // 优化显示效果
        var theme = actualSkinType switch {
            SkinType.Default => new ResourceDictionary {
                Source = new("/Themes/MyLight.xaml", UriKind.Relative)
            },
            SkinType.Dark => new ResourceDictionary {
                Source = new("/Themes/MyDark.xaml", UriKind.Relative)
            },
            SkinType.Violet => new ResourceDictionary {
                Source = new("/Themes/MyViolet.xaml", UriKind.Relative)
            },
            _ => throw new NotImplementedException()
        };

        App.Current.Resources.MergedDictionaries.Add(theme);

        CurrentSkinTypeMode = skin;
        CurrentSkinType = actualSkinType;

        ThemeChanged?.Invoke(null, skin);
        IsThemeChanging = false;
    }

    #region Static Properties Change Notification

    public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged = delegate { };

    private static void OnStaticPropertyChanged([CallerMemberName] string? staticPropertyName = null)
    {
        StaticPropertyChanged(null, new PropertyChangedEventArgs(staticPropertyName));
    }

    #endregion Static Properties Change Notification
}
