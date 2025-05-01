using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Firefly.Helpers;

namespace Firefly.Behaviors;

/// <summary>
/// <see href="https://stackoverflow.com/a/42047117/4380178">How to enable horizontal scrolling with mouse?</see>
/// </summary>
public static class ScrollViewerHelper
{
    public static readonly DependencyProperty ShiftWheelScrollsHorizontallyProperty =
        DependencyProperty.RegisterAttached(
            "ShiftWheelScrollsHorizontally",
            typeof(bool),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(false, UseHorizontalScrollingChangedCallback));

    public static readonly DependencyProperty ShiftWheelScrollSpeedProperty =
        DependencyProperty.RegisterAttached(
            "ShiftWheelScrollSpeed",
            typeof(double),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(1.0));

    public static bool GetShiftWheelScrollsHorizontally(UIElement element)
    {
        return (bool)element.GetValue(ShiftWheelScrollsHorizontallyProperty);
    }

    public static double GetShiftWheelScrollSpeed(UIElement element)
    {
        return (double)element.GetValue(ShiftWheelScrollSpeedProperty);
    }

    public static void SetShiftWheelScrollsHorizontally(UIElement element, bool value)
    {
        element.SetValue(ShiftWheelScrollsHorizontallyProperty, value);
    }

    // 默认滚动速率为 1.0
    public static void SetShiftWheelScrollSpeed(UIElement element, double value)
    {
        element.SetValue(ShiftWheelScrollSpeedProperty, value);
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
    {
        if (Keyboard.Modifiers != ModifierKeys.Shift)
        {
            return;
        }

        if (sender is not UIElement d)
        {
            return;
        }

        var scrollViewer = sender as ScrollViewer ?? VisualTreeHelperEx.FindVisualChild<ScrollViewer>(d);

        if (scrollViewer is null)
        {
            return;
        }

        double scrollSpeed = GetShiftWheelScrollSpeed(d);
        double offset = scrollSpeed * (args.Delta > 0 ? -1 : 1);

        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (offset * 48));

        args.Handled = true;
    }

    private static void UseHorizontalScrollingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            throw new Exception("Attached property must be used with UIElement.");
        }

        if ((bool)e.NewValue)
        {
            element.PreviewMouseWheel += OnPreviewMouseWheel;
        }
        else
        {
            element.PreviewMouseWheel -= OnPreviewMouseWheel;
        }
    }
}
