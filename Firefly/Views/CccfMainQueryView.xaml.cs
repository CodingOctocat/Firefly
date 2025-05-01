using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Firefly.Models.Messages;
using Firefly.ViewModels;

using HandyControl.Tools;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// CccfMainQueryView.xaml 的交互逻辑
/// </summary>
public partial class CccfMainQueryView : UserControl
{
    private static readonly Thickness _bottomPadding = new(6, 6, 6, 12);

    private static readonly Thickness _defaultPadding = new(6);

    private bool _isAtBottom;

    private ScrollViewer? _scrollViewer;

    public CccfMainQueryViewModel ViewModel => (CccfMainQueryViewModel)DataContext;

    public CccfMainQueryView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<CccfMainQueryViewModel>();

        WeakReferenceMessenger.Default.Register<RequestMessage<ScrollBarOffsetMessage>, string>(this, "RequestCccfQueryResultsScrollOffset",
            (r, m) =>
                m.Reply(new ScrollBarOffsetMessage() {
                    VerticalOffset = _scrollViewer?.VerticalOffset ?? 0
                })
            );

        WeakReferenceMessenger.Default.Register<ScrollBarOffsetMessage, string>(this, "RestoreCccfQueryResultsScrollOffset",
            (r, m) => {
                if (_scrollViewer is null)
                {
                    return;
                }

                Dispatcher.Invoke(() => {
                    if (m.HorizontalOffset >= 0)
                    {
                        _scrollViewer.ScrollToHorizontalOffset(m.HorizontalOffset);
                    }

                    if (m.VerticalOffset >= 0)
                    {
                        _scrollViewer.ScrollToVerticalOffset(m.VerticalOffset);
                    }
                }, DispatcherPriority.Render);
            });
    }

    private void DgQueryResults_Loaded(object sender, RoutedEventArgs e)
    {
        _scrollViewer = VisualHelper.GetChild<ScrollViewer>(dgQueryResults);
    }

    private void DgQueryResults_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.VerticalOffset > 0 && (_scrollViewer?.ScrollableHeight - e.VerticalOffset) < 1)
        {
            if (!_isAtBottom)
            {
                dgQueryResults.Padding = _bottomPadding;
                _scrollViewer?.ScrollToBottom();
                _isAtBottom = true;
                divAtBottom.Visibility = Visibility.Visible;
            }
        }
        else
        {
            dgQueryResults.Padding = _defaultPadding;
            _isAtBottom = false;
            divAtBottom.Visibility = Visibility.Collapsed;
        }
    }

    private void Pagination_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        ViewModel.CanExecutePageUpdated = true;
    }

    private void Pagination_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (grdPageInfo.ActualWidth + pagination.ActualWidth + 8 > grdPagination.ActualWidth)
        {
            grdPageInfo.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetRow(pagination, 1);
            pagination.HorizontalAlignment = HorizontalAlignment.Center;
        }
        else
        {
            grdPageInfo.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetRow(pagination, 0);
            pagination.HorizontalAlignment = HorizontalAlignment.Right;
        }

        double height = Math.Max(grdPageInfo.ActualHeight, pagination.ActualHeight);
        grdPageInfo.Height = height;
        pagination.Height = height;
    }

    private void QueryModeTabRadioButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.FocusOnQueryBar();
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ActualWidth < 610)
        {
            Grid.SetRow(smartQueryBar, 1);
            Grid.SetColumn(smartQueryBar, 0);
            Grid.SetColumnSpan(smartQueryBar, 2);
            smartQueryBar.Margin = new Thickness(0, 4, 0, 0);
        }
        else
        {
            Grid.SetRow(smartQueryBar, 0);
            Grid.SetColumn(smartQueryBar, 1);
            Grid.SetColumnSpan(smartQueryBar, 1);
            smartQueryBar.Margin = new Thickness(4, 0, 0, 0);
        }
    }
}
