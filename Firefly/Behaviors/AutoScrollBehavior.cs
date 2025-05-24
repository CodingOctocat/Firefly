using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using Firefly.Helpers;

namespace Firefly.Behaviors;

public static class AutoScrollBehavior
{
    public static readonly DependencyProperty AllowPauseProperty =
        DependencyProperty.RegisterAttached(
            "AllowPause",
            typeof(bool),
            typeof(AutoScrollBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(AutoScrollBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetAllowPause(DependencyObject obj)
    {
        return (bool)obj.GetValue(AllowPauseProperty);
    }

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetAllowPause(DependencyObject obj, bool value)
    {
        obj.SetValue(AllowPauseProperty, value);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl || itemsControl.ItemsSource is not INotifyCollectionChanged notifyCollection)
        {
            return;
        }

        var scrollViewer = VisualTreeHelperEx.FindVisualChild<ScrollViewer>(itemsControl);

        if (scrollViewer is null)
        {
            return;
        }

        bool allowPause = GetAllowPause(itemsControl);
        bool autoScroll = true;

        void ScrollChangedHandler(object s, ScrollChangedEventArgs args)
        {
            autoScroll = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight || !allowPause;
        }

        scrollViewer.ScrollChanged += ScrollChangedHandler;

        void CollectionChangedHandler(object? s, NotifyCollectionChangedEventArgs args)
        {
            if (autoScroll && args.Action == NotifyCollectionChangedAction.Add)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        notifyCollection.CollectionChanged += CollectionChangedHandler;

        void UnloadedHandler(object s, RoutedEventArgs args)
        {
            scrollViewer.ScrollChanged -= ScrollChangedHandler;
            notifyCollection.CollectionChanged -= CollectionChangedHandler;
            itemsControl.Unloaded -= UnloadedHandler;
        }

        itemsControl.Unloaded += UnloadedHandler;
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
