using System.Windows.Controls;

using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// DropZoneView.xaml 的交互逻辑
/// </summary>
public partial class DropZoneView : UserControl
{
    public DropZoneView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<FireflyViewModel>();
    }
}
