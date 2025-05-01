using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Messaging;

using Firefly.Models.Messages;

namespace Firefly.Views;

/// <summary>
/// CccfSmartQueryBarView.xaml 的交互逻辑
/// </summary>
public partial class CccfSmartQueryBarView : UserControl
{
    public CccfSmartQueryBarView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        WeakReferenceMessenger.Default.Register<FocusOnSmartQueryBarMessage>(this,
            (r, m) => RestoreFocus(m.NeedsSelectAll));
    }

    private void RestoreFocus(bool needsSelectAll = false)
    {
        searchBar.Focus();
        Keyboard.Focus(searchBar);

        if (needsSelectAll && !String.IsNullOrEmpty(searchBar.Text))
        {
            searchBar.SelectAll();
        }
    }

    private void SearchBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is false)
        {
            return;
        }

        RestoreFocus();
    }
}
