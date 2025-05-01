using System.Windows;
using System.Windows.Controls;

namespace Firefly.Behaviors;

public static class InputElementContextMenuBehavior
{
    public static readonly DependencyProperty UseCustomStyleProperty =
        DependencyProperty.RegisterAttached(
            "UseCustomStyle",
            typeof(bool),
            typeof(InputElementContextMenuBehavior),
            new PropertyMetadata(false, OnUseCustomStyleChanged));

    public static bool GetUseCustomStyle(DependencyObject element)
    {
        return (bool)element.GetValue(UseCustomStyleProperty);
    }

    public static void SetUseCustomStyle(DependencyObject element, bool value)
    {
        element.SetValue(UseCustomStyleProperty, value);
    }

    private static void OnUseCustomStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Control control && (bool)e.NewValue)
        {
            control.Loaded += (s, e) => {
                if (control.Template.FindName("PART_TextBox", control) is FrameworkElement fe)
                {
                    fe.ContextMenu = (ContextMenu)Application.Current.FindResource("InputElementContextMenu");
                }
            };
        }
    }
}
