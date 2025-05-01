using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Firefly.Helpers;
using Firefly.Models;
using Firefly.Services.Abstractions;

using Microsoft.Xaml.Behaviors;

namespace Firefly.Behaviors;

public class DataGridScrollFindableToCenterBehavior : Behavior<DataGrid>, IScrollFindableToCenter
{
    // Using a DependencyProperty as the backing store for Client.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ClientProperty =
        DependencyProperty.Register(
            "Client",
            typeof(ISupportScrollFindableToCenterBehavior),
            typeof(DataGridScrollFindableToCenterBehavior),
            new PropertyMetadata(null));

    // Using a DependencyProperty as the backing store for FindableScopes.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FindableScopesProperty =
        DependencyProperty.Register(
            "FindableScopes",
            typeof(object),
            typeof(DataGridScrollFindableToCenterBehavior),
            new PropertyMetadata(null, OnFindableScopesChanged));

    public ISupportScrollFindableToCenterBehavior Client
    {
        get => (ISupportScrollFindableToCenterBehavior)GetValue(ClientProperty);
        set => SetValue(ClientProperty, value);
    }

    public object FindableScopes
    {
        get => GetValue(FindableScopesProperty);
        set => SetValue(FindableScopesProperty, value);
    }

    public IEnumerable<IPageFindable<IPageFindableInfo>>? ItemsSource
    {
        get
        {
            if (AssociatedObject.ItemsSource is ICollectionView collectionView)
            {
                return collectionView.SourceCollection as IEnumerable<IPageFindable<IPageFindableInfo>>;
            }

            return AssociatedObject.ItemsSource as IEnumerable<IPageFindable<IPageFindableInfo>>;
        }
    }

    public void ScrollToCenter(int itemIndex, int index)
    {
        if (itemIndex < 0 || index < 1)
        {
            return;
        }

        var scrollViewer = VisualTreeHelperEx.FindVisualChild<ScrollViewer>(AssociatedObject);

        if (scrollViewer is null)
        {
            return;
        }

        AssociatedObject.SelectedIndex = itemIndex;

        if (AssociatedObject.RowDetailsTemplate is not null)
        {
            AssociatedObject.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        }

        var element = Dispatcher.CurrentDispatcher.Invoke(
            () => VisualTreeHelperEx.FindVisualChild<FrameworkElement>(scrollViewer, x => x.DataContext is IPageFindableInfo info && info.Index == index),
            DispatcherPriority.Render);

        int timeoutCounter = 0;
        const int maxTimeout = 10;

        // 结果不在可视化树（虚拟化），或者不在视口但尚未被虚拟化（视图缓存）：滚动到结果所在行
        do
        {
            AssociatedObject.ScrollIntoView(AssociatedObject.Items[itemIndex]);

            element = Dispatcher.CurrentDispatcher.Invoke(
                () => VisualTreeHelperEx.FindVisualChild<FrameworkElement>(scrollViewer, x => x.DataContext is IPageFindableInfo info && info.Index == index),
                DispatcherPriority.Render);

            timeoutCounter++;
        } while (element is null && timeoutCounter < maxTimeout);

        if (element is null)
        {
            return;
        }

        // DataGrid 的 ScrollViewer 包含列标题部分，需要获取内部的 ScrollContentPresenter 计算实际的视口
        // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/datagrid-styles-and-templates?#datagrid-controltemplate-example
        var scrollContentPresenter = VisualTreeHelperEx.FindVisualChild<ScrollContentPresenter>(scrollViewer);
        FrameworkElement scrollContainer = scrollContentPresenter is null ? scrollViewer : scrollContentPresenter;

        // 结果在可视化树，但不在视口：将结果滚动到视口中心
        if (!IsElementInViewport(scrollContainer, element))
        {
            // 先将结果放入到视口
            element.BringIntoView();

            Dispatcher.CurrentDispatcher.Invoke(() => {
                // 判断结果位于视口中心的上方还是下方，进行相应的滚动偏移，使其到视口中心
                var transform = element.TransformToVisual(scrollViewer);
                var position = transform.Transform(new Point(0, 0));
                double y = position.Y;

                double vOffset = (scrollViewer.ViewportHeight / 2) - (element.ActualHeight / 2);

                if (y + (element.ActualHeight / 2) > scrollViewer.ViewportHeight / 2) // 下半区域
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + vOffset);
                }
                else // 上半区域
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - vOffset);
                }
            }, DispatcherPriority.Render);
        }
        else // 结果在视口：直接增强高亮效果
        {
            // 通过样式触发器实现
        }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        Dispatcher.BeginInvoke(() => {
            if (Client is not null)
            {
                Client.CenterScroller = this;
            }
        }, DispatcherPriority.Loaded);

        AssociatedObject.Loaded += AddValueChangedHandler;
        AssociatedObject.Unloaded -= AddValueChangedHandler;
    }

    protected override void OnDetaching()
    {
        if (Client is not null)
        {
            if (ReferenceEquals(Client.CenterScroller, this))
            {
                Client.CenterScroller = null!;
                Client.FindableScopes = null;
            }
        }

        base.OnDetaching();
    }

    private static bool IsElementInViewport(FrameworkElement container, FrameworkElement element)
    {
        if (!element.IsVisible)
        {
            return false;
        }

        var transform = element.TransformToAncestor(container);
        var elementBounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        var containerBounds = new Rect(0, 0, container.ActualWidth, container.ActualHeight);

        return containerBounds.Contains(elementBounds);
    }

    private static void OnFindableScopesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGridScrollFindableToCenterBehavior behavior)
        {
            if (behavior.Client is not null)
            {
                if (e.NewValue is IEnumerable<IPageFindable<IPageFindableInfo>> pageFindables)
                {
                    behavior.Client.FindableScopes = pageFindables;
                }
                else if (e.NewValue is IPageFindable<IPageFindableInfo> pageFindable)
                {
                    behavior.Client.FindableScopes = [pageFindable];
                }
                else
                {
                    behavior.Client.FindableScopes = behavior.AssociatedObject.ItemsSource as IEnumerable<IPageFindable<IPageFindableInfo>>;
                }
            }
        }
    }

    private void AddValueChangedHandler(object sender, RoutedEventArgs e)
    {
        OnItemsSourceChanged(sender, e);

        var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
        dpd?.AddValueChanged(AssociatedObject, OnItemsSourceChanged);
    }

    private void OnItemsSourceChanged(object? sender, EventArgs e)
    {
        Client.DataSource = ItemsSource;

        if (FindableScopes is null)
        {
            Client!.FindableScopes = ItemsSource;
        }
    }
}
