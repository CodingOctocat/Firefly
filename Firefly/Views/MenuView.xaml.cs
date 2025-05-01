using System.Windows.Controls;

using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// MenuBarView.xaml 的交互逻辑
/// </summary>
public partial class MenuView : UserControl
{
    public MenuView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<MenuViewModel>();
    }
}
