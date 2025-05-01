using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using Firefly.Helpers;

namespace Firefly.Behaviors;

public static class AutoScrollBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(AutoScrollBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
        {
            return;
        }

        var scrollViewer = VisualTreeHelperEx.FindVisualChild<ScrollViewer>(itemsControl);

        if (scrollViewer is null)
        {
            return;
        }

        bool autoScroll = true;

        scrollViewer.ScrollChanged += (s, ev) => {
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                autoScroll = true;
            }
            else
            {
                autoScroll = false;
            }
        };

        if (itemsControl.ItemsSource is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged += (s, ev) => {
                if (autoScroll)
                {
                    scrollViewer.ScrollToEnd();
                }
            };
        }
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsControl itemsControl)
        {
            if ((bool)e.NewValue)
            {
                itemsControl.Loaded += ItemsControl_Loaded;
            }
            else
            {
                itemsControl.Loaded -= ItemsControl_Loaded;
            }
        }
    }
}
