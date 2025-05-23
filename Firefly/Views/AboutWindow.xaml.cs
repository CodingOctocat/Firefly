﻿using System.Windows.Controls;
using System.Windows.Input;

using Firefly.Helpers;
using Firefly.Models;
using Firefly.ViewModels;

using HcWindow = HandyControl.Controls.Window;

namespace Firefly.Views;

/// <summary>
/// AboutWindow.xaml 的交互逻辑
/// </summary>
public partial class AboutWindow : HcWindow
{
    public AboutViewModel ViewModel { get; }

    public AboutWindow(AboutViewModel vm)
    {
        InitializeComponent();
        ViewModel = vm;

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = vm;
    }

    private void LbAppDependencies_Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            var item = (ListBoxItem)sender;
            var dep = (PackageDependency)item.DataContext;
            UriHelper.OpenUri($"https://www.nuget.org/packages/{dep.Name}");
        }
    }

    private async void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await ViewModel.LoadedCommand.ExecuteAsync(null);
    }
}
