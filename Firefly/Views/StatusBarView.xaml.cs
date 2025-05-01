using System.Windows.Controls;

using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// StatusBarView.xaml 的交互逻辑
/// </summary>
public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<StatusBarViewModel>();
    }
}
