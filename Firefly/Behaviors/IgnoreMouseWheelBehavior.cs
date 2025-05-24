using System.Windows;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace Firefly.Behaviors;

/// <summary>
/// 忽略 <seealso cref="UIElement"/> 的鼠标滚轮行为，兼容附加属性模式，支持按住 shift 键临时禁用此行为。
/// <para>
/// <see href="https://stackoverflow.com/a/15904265/4380178">Bubbling scroll events from a ListView to its parent</see>
/// </para>
/// </summary>
public sealed class IgnoreMouseWheelBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(IgnoreMouseWheelBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;

        base.OnDetaching();
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement uiElement)
        {
            if ((bool)e.NewValue)
            {
                uiElement.PreviewMouseWheel += OnPreviewMouseWheel;
            }
            else
            {
                uiElement.PreviewMouseWheel -= OnPreviewMouseWheel;
            }
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            return;
        }

        e.Handled = true;

        var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
            RoutedEvent = UIElement.MouseWheelEvent
        };

        ((UIElement)sender).RaiseEvent(args);
    }

    private void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            return;
        }

        e.Handled = true;

        var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
            RoutedEvent = UIElement.MouseWheelEvent
        };

        AssociatedObject.RaiseEvent(args);
    }
}
