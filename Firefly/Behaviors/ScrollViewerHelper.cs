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

    public static bool GetShiftWheelScrollsHorizontally(DependencyObject obj)
    {
        return (bool)obj.GetValue(ShiftWheelScrollsHorizontallyProperty);
    }

    public static double GetShiftWheelScrollSpeed(DependencyObject obj)
    {
        return (double)obj.GetValue(ShiftWheelScrollSpeedProperty);
    }

    public static void SetShiftWheelScrollsHorizontally(DependencyObject obj, bool value)
    {
        obj.SetValue(ShiftWheelScrollsHorizontallyProperty, value);
    }

    public static void SetShiftWheelScrollSpeed(DependencyObject obj, double value)
    {
        obj.SetValue(ShiftWheelScrollSpeedProperty, value);
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
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
        double offset = scrollSpeed * (e.Delta > 0 ? -1 : 1);

        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (offset * 48));

        e.Handled = true;
    }

    private static void UseHorizontalScrollingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement uiElement)
        {
            throw new Exception("Attached property must be used with UIElement.");
        }

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
