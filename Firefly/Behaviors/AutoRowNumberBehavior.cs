using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Firefly.Helpers;

namespace Firefly.Behaviors;

public static class AutoRowNumberBehavior
{
    public static readonly DependencyProperty EnableAutoRowNumbersProperty =
        DependencyProperty.RegisterAttached(
            "EnableAutoRowNumbers",
            typeof(bool),
            typeof(AutoRowNumberBehavior),
            new PropertyMetadata(false, OnEnableAutoRowNumbersChanged));

    public static bool GetEnableAutoRowNumbers(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnableAutoRowNumbersProperty);
    }

    public static void SetEnableAutoRowNumbers(DependencyObject obj, bool value)
    {
        obj.SetValue(EnableAutoRowNumbersProperty, value);
    }

    private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
        {
            return;
        }

        if (itemsControl.ItemsSource is not INotifyCollectionChanged collection)
        {
            return;
        }

        // DispatcherPriority.Render 确保 UI 更新后再执行 RefreshRowNumbers()
        collection.CollectionChanged += (s, args)
            => itemsControl.Dispatcher.Invoke(() => RefreshRowNumbers(itemsControl), DispatcherPriority.Render);

        RefreshRowNumbers(itemsControl);
    }

    private static void OnEnableAutoRowNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
