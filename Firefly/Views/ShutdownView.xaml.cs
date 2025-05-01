using System.Windows;
using System.Windows.Controls;

using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// ShutdownView.xaml 的交互逻辑
/// </summary>
public partial class ShutdownView : UserControl
{
    public ShutdownViewModel ViewModel => (ShutdownViewModel)DataContext;

    public ShutdownView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<ShutdownViewModel>();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.DelayShutdownCommand.Execute(null);
    }
}
