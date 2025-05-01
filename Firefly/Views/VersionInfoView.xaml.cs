using System;
using System.Windows.Controls;

using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Views;

/// <summary>
/// VersionInfoView.xaml 的交互逻辑
/// </summary>
public partial class VersionInfoView : UserControl
{
    public VersionInfoView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<VersionInfoViewModel>();
    }
}
