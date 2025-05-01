using System;
using System.Windows.Controls;

using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// FireflyView.xaml 的交互逻辑
/// </summary>
public partial class FireflyView : UserControl
{
    public FireflyView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<FireflyViewModel>();
    }
}
