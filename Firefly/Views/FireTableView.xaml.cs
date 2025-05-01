using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.Messaging;

using Firefly.Helpers;
using Firefly.Models.Messages;
using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// FireTableView.xaml 的交互逻辑
/// </summary>
public partial class FireTableView : UserControl
{
    private ContextMenu? _currentContextMenu;

    private bool _isRightClick = false;

    private object? _lastClickedItem;

    private object? _selectedItemBeforeRightClick;

    public FireTableView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<FireflyViewModel>();

        WeakReferenceMessenger.Default.Register<ActionMessage, string>(this, "ResetFireTableAutoColumnWidths",
            (r, m) => ResetAutoColumnWidths());
    }

    public void ResetAutoColumnWidths()
    {
        // DispatcherPriority.Render 确保 UI 更新后再调整列宽
        // HACK: 数据源加载后，必须重置列宽，否则可能会导致列宽无法调整，或者调整时实际列宽发生大幅变化
        dgFirefly.Dispatcher.Invoke(() => {
            foreach (var column in dgFirefly.Columns)
            {
                column.Width = new DataGridLength(0);
            }

            foreach (var column in dgFirefly.Columns)
            {
                column.Width = DataGridLength.Auto;
            }
        }, DispatcherPriority.Render);
    }

    private void DgFirefly_ColumnReordered(object sender, DataGridColumnEventArgs e)
    {
        if (e.Column.DisplayIndex == 0 && e.Column.Header.ToString() != "#")
        {
            e.Column.DisplayIndex = 1;
        }
    }

    private void DgFirefly_ContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
        _currentContextMenu = null;
        _isRightClick = false;
    }

    private void DgFirefly_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // DataGrid 如何禁用右键选中行但不影响单元格的上下文菜单？
        // https://github.com/BYJRK/DotNet-Discussions/discussions/415
        // TODO: 特定情况下长按右键仍然会选中行？
        if (e.OriginalSource is DependencyObject dep)
        {
            var row = VisualTreeHelperEx.FindVisualParent<DataGridRow>(dep);

            if (row is not null)
            {
                _isRightClick = true;
                _selectedItemBeforeRightClick = dgFirefly.SelectedItem;
            }

            var cell = VisualTreeHelperEx.FindVisualParent<DataGridCell>(dep);

            if (cell is not null)
            {
                _currentContextMenu = cell.ContextMenu;
            }
        }
    }

    private void DgFirefly_Row_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject dep)
        {
            var details = VisualTreeHelperEx.FindVisualParent<DataGridDetailsPresenter>(dep);

            if (details is not null)
            {
                return;
            }
        }

        object clickedItem = ((FrameworkElement)sender).DataContext;

        if (_currentContextMenu is not null)
        {
            _currentContextMenu.IsOpen = false;
            _currentContextMenu = null;
            _lastClickedItem = clickedItem;
            e.Handled = true;

            return;
        }

        if (clickedItem == _lastClickedItem)
        {
            if (dgFirefly.RowDetailsVisibilityMode == DataGridRowDetailsVisibilityMode.VisibleWhenSelected
                && dgFirefly.SelectedItem == clickedItem)
            {
                dgFirefly.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            }
            else
            {
                dgFirefly.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }

            dgFirefly.SelectedItem = clickedItem;
        }
        else
        {
            dgFirefly.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
        }

        _lastClickedItem = clickedItem;
    }

    private void DgFirefly_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.VerticalChange > 0)
        {
            gotoTop.Visibility = Visibility.Collapsed;
        }
        else
        {
            gotoTop.Visibility = Visibility.Visible;
        }
    }

    private void DgFirefly_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRightClick)
        {
            dgFirefly.SelectedItem = _selectedItemBeforeRightClick;
            _isRightClick = false;
        }
    }
}
