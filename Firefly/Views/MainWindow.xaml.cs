using System;
using System.IO;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Messaging;

using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Services.Abstractions;
using Firefly.ViewModels;

using Microsoft.Extensions.DependencyInjection;

using HcWindow = HandyControl.Controls.Window;

namespace Firefly.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : HcWindow
{
    private readonly IViewSwitcher _viewSwitcher = null!;

    private int _escapePressCount;

    private long _lastEscapePressTick;

    public static Version? Version => App.Version;

    public string ReleasedLongTime => File.GetLastWriteTime(GetType().Assembly.Location).ToString();

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        DataContext = vm;
        _viewSwitcher = App.Current.Services.GetRequiredService<IViewSwitcher>();

        WeakReferenceMessenger.Default.Register<FocusOnMainWindowMessage>(this,
            (r, m) => {
                Focus();
                FocusManager.SetFocusedElement(this, this);
            });
    }

    protected override void OnClosed(EventArgs e)
    {
        WindowHelper.CloseGrowlNotificationIfNecessary();

        base.OnClosed(e);
    }

    private void CccfMainQueryView_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            _viewSwitcher.NotifyRenderCompleted(ActiveView.CccfQuery);
        }
    }

    private void FireflyView_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            _viewSwitcher.NotifyRenderCompleted(ActiveView.Firefly);
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // 焦点位于 DataGridCell 或 TextBox 时，部分快捷键将失效，需要将焦点移到主窗口
        if (e.Key == Key.Escape)
        {
            _escapePressCount++;

            if (_escapePressCount == 1)
            {
                _lastEscapePressTick = Environment.TickCount;
            }
            else if (_escapePressCount == 2)
            {
                if ((Environment.TickCount - _lastEscapePressTick + Int32.MaxValue + 1) % (Int32.MaxValue + 1L) < 250)
                {
                    WeakReferenceMessenger.Default.Send(new FocusOnMainWindowMessage());
                }

                _escapePressCount = 0;
            }
        }
    }
}
