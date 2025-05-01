using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Messaging;

using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.ViewModels;

using Hardcodet.Wpf.TaskbarNotification;

using Microsoft.Extensions.DependencyInjection;

using HcMessageBox = HandyControl.Controls.MessageBox;
using HcWindow = HandyControl.Controls.Window;
using Visibility = System.Windows.Visibility;

namespace Firefly.Views;

/// <summary>
/// CccfScraperWindow.xaml 的交互逻辑
/// </summary>
public partial class CccfScraperWindow : HcWindow
{
    private const double InitHeight = 251;

    private const double InitWidth = 640;

    public CccfScraperViewModel ViewModel => (CccfScraperViewModel)DataContext;

    public CccfScraperWindow()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = App.Current.Services.GetRequiredService<CccfScraperViewModel>();
        WeakReferenceMessenger.Default.Register<TrayMessage, string>(this, "ShowCccfScraperWindow",
            (r, m) => ShowWindow());

        WeakReferenceMessenger.Default.Register<TrayMessage, string>(this, "ShowDbSyncCompletedBalloonTip",
            (r, m) => {
                if (!myNotifyIcon.IsDisposed)
                {
                    myNotifyIcon.ShowBalloonTip(App.AppFullName, "同步已完成。", BalloonIcon.Info);
                }
            });
    }

    protected override void OnClosed(EventArgs e)
    {
        WindowHelper.CloseGrowlNotificationIfNecessary();

        base.OnClosed(e);
    }

    private void BtnMinimizeToTray_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void DgLogs_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (dgLogs.IsVisible)
        {
            SizeToContent = SizeToContent.Manual;
            Width = InitWidth;
            MinHeight = 600;
            ResizeMode = ResizeMode.CanResize;

            var screen = SystemParameters.WorkArea;
            double screenHeight = screen.Height;
            double windowBottom = Top + ActualHeight;

            if (windowBottom > screenHeight)
            {
                double overflow = windowBottom - screenHeight;
                Top -= overflow;

                if (Top < 0)
                {
                    Top = 0;
                }
            }
        }
        else
        {
            WindowState = WindowState.Normal;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            Width = InitWidth;
            MinHeight = InitHeight;
            Height = InitHeight;
        }
    }

    private void DgLogs_Row_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            var row = (DataGridRow)sender;
            var log = (CccfScraperLog)row.DataContext;
            string msg = $"{log.Timestamp:MM/dd, hh:mm:ss.fff} @ {log.PageNumber}\n\n{log.Message}";
            HcMessageBox.Info(msg, $"{App.AppName} - #{log.Index}");
        }
    }

    private void MiClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MyNotifyIcon_ShowWindow(object sender, RoutedEventArgs e)
    {
        ShowWindow();
    }

    private void ShowWindow()
    {
        if (Visibility != Visibility.Visible)
        {
            Show();
        }
        else
        {
            WindowState = WindowState.Normal;
            Activate();
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        myNotifyIcon.Dispose();
    }
}
