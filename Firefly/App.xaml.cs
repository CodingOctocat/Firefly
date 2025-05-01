using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Firefly.Extensions;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Properties;
using Firefly.ServiceProviders;
using Firefly.Views;

using HandyControl.Controls;
using HandyControl.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using HcMessageBox = HandyControl.Controls.MessageBox;

namespace Firefly;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    #region Fields

    public const string AppFullName = "Firefly (萤火虫)";

    public const string AppName = "Firefly";

    public static readonly string AppSettingsFileName = "appsettings.json";

    private readonly IHost _host;

    private Mutex? _mutex;

    #endregion Fields

    #region Properties

    public static bool CanForceClose { get; set; }

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use.
    /// </summary>
    public static new App Current => (App)Application.Current;

    public static bool IsBusy { get; set; }

    public static bool IsInDebugMode { get; }

    public static bool IsInDesignMode => Application.Current is not App;

    public static Version? Version => Assembly.GetEntryAssembly()?.GetName().Version;

    public static string VersionString => $"v{Version?.Major}.{Version?.Minor}.{Version?.Build}";

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    #endregion Properties

    #region Constructors

    static App()
    {
#if DEBUG
        IsInDebugMode = true;
        AppSettingsFileName = "appsettings.Development.json";
#endif
    }

    public App()
    {
        InitializeComponent();

        _host = new HostBuilder()
            .ConfigureAppConfiguration((context, configurationBuilder) => configurationBuilder
                    .SetBasePath(context.HostingEnvironment.ContentRootPath)
                    .AddJsonFile(AppSettingsFileName, optional: true, reloadOnChange: false))
            .ConfigureServices((ctx, services) => {
                services.AddHttpClientServices(ctx)
                    .AddDbContextServices(ctx)
                    .AddServices()
                    .AddViewModels(ctx)
                    .AddViews(ctx);

                services.BuildServiceProvider(validateScopes: true);
            })
            .Build();

        ThemeWatcher.WindowsThemeChanged += ThemeWatcher_WindowsThemeChanged;
        ThemeWatcher.StartThemeWatching();
    }

    #endregion Constructors

    #region Override Methods

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        ThemeWatcher.WindowsThemeChanged -= ThemeWatcher_WindowsThemeChanged;

        base.OnExit(e);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        const string mutexName = "CodingNinja.Firefly";

        _mutex = new Mutex(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            HcMessageBox.Show("程序已经在运行中...", AppName);

            _mutex.Close();
            _mutex = null;
            Current.Shutdown();
        }

        base.OnStartup(e);

        // 全局异常处理
        SetupExceptionHandling();

        if (Settings.Default.UpgradeRequired)
        {
            Settings.Default.Upgrade();
            Settings.Default.Save();
            Settings.Default.Reload();
            Settings.Default.UpgradeRequired = false;
            Settings.Default.Save();
        }

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Closed += MainWindow_Closed;
        mainWindow.Show();

        await Task.CompletedTask;
    }

    #endregion Override Methods

    #region Methods

    private static void RestartApp()
    {
        var module = Process.GetCurrentProcess().MainModule;

        if (module is null)
        {
            Growl.Error("重启失败！");
        }
        else
        {
            Process.Start(module.FileName);
            CanForceClose = true;
            Current.Shutdown();
        }
    }

    private static async void ShowUnhandledException(Exception exception, string handler)
    {
        string logStatus;

        try
        {
            var info = Directory.CreateDirectory("logs");
            var exc = new SerializableException(exception, handler);
            string logFileName = $"{exc.Exception.Type.Name}_{exc.Timestamp:yyyyMMdd_HHmmss}.log";
            string logFilePath = Path.Combine(info.FullName, logFileName);
            logStatus = $@"详细信息: .\logs\{logFileName}";

            await File.WriteAllTextAsync(logFilePath, exc.ToString());
        }
        catch
        {
            logStatus = "日志记录失败！";
        }

        string prettyEx = exception.Pretty();
        string msg = $"程序已崩溃~~~\n\n[{handler}]\n<{exception.GetType()}>\n{prettyEx}\n{logStatus}";

        // 窗口未处于活动状态可能导致 Growl 不显示
        Current.MainWindow?.Activate();

        var growlInfo = new GrowlInfo {
            Message = msg,
            ShowDateTime = true,
            ConfirmStr = "重启程序",
            CancelStr = "忽略",
            IsCustom = true,
            IconKey = ResourceToken.FatalGeometry,
            IconBrushKey = ResourceToken.DangerBrush,
            ActionBeforeClose = isConfirm => {
                if (isConfirm)
                {
                    RestartApp();
                }

                return true;
            }
        };

        if (Current.MainWindow is MainWindow)
        {
            Growl.Ask(growlInfo);
        }
        else
        {
            var result = HcMessageBox.Show(
                 $"{msg}\n\n是否要重启程序？",
                 AppFullName,
                 MessageBoxButton.YesNo,
                 MessageBoxImage.Error,
                 MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                RestartApp();
            }
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        if (_mutex is not null)
        {
            _mutex.ReleaseMutex();
            _mutex.Close();
        }
    }

    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
           ShowUnhandledException((Exception)e.ExceptionObject, nameof(AppDomain.CurrentDomain.UnhandledException));

        DispatcherUnhandledException += (s, e) => {
            ShowUnhandledException(e.Exception, nameof(DispatcherUnhandledException));
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, e) => {
            ShowUnhandledException(e.Exception, nameof(TaskScheduler.UnobservedTaskException));
            e.SetObserved();
        };
    }

    private void ThemeWatcher_WindowsThemeChanged(object? sender, ThemeWatcher.ThemeChangedArgs e)
    {
        if (ThemeManager.CurrentSkinTypeMode is null)
        {
            ThemeManager.UpdateSkin(null);
        }
    }

    #endregion Methods
}
