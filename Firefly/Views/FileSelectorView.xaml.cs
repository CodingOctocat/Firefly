using System.Windows.Controls;

using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// FileSelectorView.xaml 的交互逻辑
/// </summary>
public partial class FileSelectorView : UserControl
{
    public FileSelectorView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<FireflyViewModel>();
    }
}
