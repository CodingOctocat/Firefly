using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Firefly.Helpers;

namespace Firefly.Behaviors;

public static class AutoRowNumberBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(AutoRowNumberBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl || itemsControl.ItemsSource is not INotifyCollectionChanged notifyCollection)
        {
            return;
        }

        void CollectionChangedHandler(object? s, NotifyCollectionChangedEventArgs args)
        {
            // DispatcherPriority.Render 确保 UI 更新后再执行 RefreshRowNumbers()
            itemsControl.Dispatcher.Invoke(() => RefreshRowNumbers(itemsControl), DispatcherPriority.Render);
        }

        notifyCollection.CollectionChanged += CollectionChangedHandler;

        void UnloadedHandler(object s, RoutedEventArgs args)
        {
            notifyCollection.CollectionChanged -= CollectionChangedHandler;
            itemsControl.Unloaded -= UnloadedHandler;
        }

        itemsControl.Unloaded += UnloadedHandler;

        RefreshRowNumbers(itemsControl);
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ItemsControl itemsControl)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            itemsControl.Loaded += ItemsControl_Loaded;
        }
        else
        {
            itemsControl.Loaded -= ItemsControl_Loaded;
        }
    }

    private static void RefreshRowNumbers(ItemsControl itemsControl)
    {
        foreach (object? item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container)
            {
                var textBlock = VisualTreeHelperEx.FindVisualChild<TextBlock>(container, x => x.Name == "RowNumberText");
                textBlock?.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            }
        }
    }
}
