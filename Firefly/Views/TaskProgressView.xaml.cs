using System.Windows.Controls;

using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// TaskProgressView.xaml 的交互逻辑
/// </summary>
public partial class TaskProgressView : UserControl
{
    public TaskProgressView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<FireflyViewModel>();
    }
}
