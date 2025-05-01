using System;

using Firefly.Helpers;
using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using HcWindow = HandyControl.Controls.Window;

namespace Firefly.Views;

/// <summary>
/// CccfDbMergeWindow.xaml 的交互逻辑
/// </summary>
public partial class CccfDbMergeWindow : HcWindow
{
    public CccfDbMergeWindow()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<CccfDbMergeViewModel>();
    }

    protected override void OnClosed(EventArgs e)
    {
        WindowHelper.CloseGrowlNotificationIfNecessary();

        base.OnClosed(e);
    }
}
