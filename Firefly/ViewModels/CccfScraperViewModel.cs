using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Firefly.Common;
using Firefly.Extensions;
using Firefly.Factories;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Models.Responses;
using Firefly.Services;
using Firefly.Services.Abstractions;
using Firefly.Services.Requests;
using Firefly.Views;

using HandyControl.Controls;
using HandyControl.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly.RateLimiting;
using Polly.Timeout;

using HcMessageBox = HandyControl.Controls.MessageBox;

namespace Firefly.ViewModels;

public partial class CccfScraperViewModel : ObservableRecipient
{
    #region Fields

    public const string Title = "Firefly (萤火虫): 同步本地数据库";

    private readonly CccfDbContext _cccfDbContext;

    private readonly ICccfService _cccfService;

    private readonly IConfiguration _configuration;

    private int _pageSize;

    private Notification? _shutdownNotification;

    private int _syncedRecords;

    private int _totalRecords;

    #endregion Fields

    #region Properties

    public CccfRequest CccfRequest { get; } = new();

    public ObservableRangeCollection<CccfScraperLog> CccfScraperLogs { get; } = [];

    public string CountdownToolTip
    {
        get
        {
            if (ProgressTimer.Countdown is null)
            {
                return $"已用时间: {(int)ProgressTimer.UsedTime.TotalHours:D2}:{ProgressTimer.UsedTime.Minutes:D2}:{ProgressTimer.UsedTime.Seconds:D2}, 预计剩余: 正在计算...";
            }

            return $"已用时间: {(int)ProgressTimer.UsedTime.TotalHours:D2}:{ProgressTimer.UsedTime.Minutes:D2}:{ProgressTimer.UsedTime.Seconds:D2}, 预计剩余: {(int)ProgressTimer.Countdown.Value.TotalHours:D2}:{ProgressTimer.Countdown.Value.Minutes:D2}:{ProgressTimer.Countdown.Value.Seconds:D2}";
        }
    }

    public string CoverageToolTip => $"[覆盖率 {LocalDbCoverage,6:P2}] {LocalDbTotalRecords:N0}/{QueryResponseTotalRecords:N0} ({QueryResponseTotalRecords - LocalDbTotalRecords:-#,#;+#,#;0})";

    [ObservableProperty]
    public partial int CurrentPage { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrayToolTip))]
    [NotifyPropertyChangedFor(nameof(TrayShortToolTip))]
    public partial FireTaskStatus FireTaskStatus { get; private set; }

    [ObservableProperty]
    public partial int Interval { get; set; } = 10000;

    public bool IsAscending => PageTo >= PageFrom;

    [ObservableProperty]
    public partial bool IsPageToLast { get; set; }

    public double LocalDbCoverage => QueryResponse?.TotalRecords == 0 ? 0d : (double)LocalDbTotalRecords / QueryResponseTotalRecords;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocalDbCoverage))]
    [NotifyPropertyChangedFor(nameof(CoverageToolTip))]
    public partial int LocalDbTotalRecords { get; private set; }

    [ObservableProperty]
    public partial int MinInterval { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SyncPages))]
    [NotifyPropertyChangedFor(nameof(SyncRecords))]
    [NotifyPropertyChangedFor(nameof(SyncPercentage))]
    public partial int PageFrom { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SyncPages))]
    [NotifyPropertyChangedFor(nameof(SyncRecords))]
    [NotifyPropertyChangedFor(nameof(SyncPercentage))]
    public partial int PageTo { get; set; }

    public ProgressTimer ProgressTimer { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocalDbCoverage))]
    [NotifyPropertyChangedFor(nameof(CoverageToolTip))]
    public partial QueryResponse<Cccf>? QueryResponse { get; private set; }

    public int QueryResponseTotalRecords => QueryResponse?.TotalRecords ?? 0;

    [ObservableProperty]
    public partial bool ShowLogs { get; set; }

    [ObservableProperty]
    public partial bool ShutdownAfterCompletion { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SyncPercentage))]
    public partial int SyncedPages { get; private set; }

    public int SyncPages => Math.Abs(PageTo - PageFrom) + 1;

    public double SyncPercentage => (double)SyncedPages / SyncPages;

    public int SyncRecords => SyncPages * _pageSize;

    public string SyncState => FireTaskStatus switch {
        FireTaskStatus.Normal => "[同步中 {0,6:P2}]",
        FireTaskStatus.Completed => "[已完成 {0,6:P2}]",
        FireTaskStatus.Cancelled => "[已暂停 {0,6:P2}]",
        FireTaskStatus.Error => "[! 错误 {0,6:P2}]",
        _ => "[> 就绪 {0,6:P2}]"
    };

    public string SyncStateToolTip => $"[已同步] {SyncToolTip}";

    public string SyncToolTip => $"页: {SyncedPages:N0}/{SyncPages:N0} ({SyncPages - SyncedPages:-#,#;+#,#;0}) | 记录: {_syncedRecords:N0}/{SyncRecords:N0} ({SyncRecords - _syncedRecords:-#,#;+#,#;0})";

    [ObservableProperty]
    public partial TaskbarItemProgressState TaskbarItemProgressState { get; private set; }

    [ObservableProperty]
    public partial bool Topmost { get; set; } = true;

    public string TrayShortToolTip => $"{Title}\n\n{String.Format(SyncState, SyncPercentage)}\n{CountdownToolTip}";

    /// <summary>
    /// Windows 托盘工具提示最多 63 个字符。
    /// </summary>
    public string TrayToolTip => $"{Title}\n\n{CoverageToolTip}\n{String.Format(SyncState, SyncPercentage)} {SyncToolTip}\n\n{CountdownToolTip}";

    #endregion Properties

    #region Constructors & Recipients

    public CccfScraperViewModel(ICccfServiceFactory cccfServiceFactory, CccfDbContext cccfDbContext, ProgressTimer progressTimer, IConfiguration configuration)
    {
        IsActive = true;

        _cccfService = cccfServiceFactory.Create(CccfServiceFactory.ScraperPipeline);
        _cccfDbContext = cccfDbContext;
        ProgressTimer = progressTimer;
        _configuration = configuration;

        int minInterval = _configuration.GetValue("CccfScraper:MinInterval", 10000);
        MinInterval = Math.Max(minInterval, 500);
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        Messenger.Register<CancelShutdownMessage, string>(this, "ShutdownViewModel",
            (r, m) => _shutdownNotification?.Close());
    }

    #endregion Constructors & Recipients

    #region Commands

    [RelayCommand]
    private void Closing(CancelEventArgs e)
    {
        if (StartCommand.IsRunning)
        {
            ShowWindow();

            var result = HcMessageBox.Show(
                "正在同步本地数据库，确定关闭？",
                App.AppName,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                StartCancelCommand.Execute(null);
            }
            else
            {
                e.Cancel = true;
            }
        }
    }

    [RelayCommand]
    private async Task LoadedAsync()
    {
        CccfScraperLog.ResetIndex();
        await _cccfDbContext.Database.EnsureCreatedAsync();
        await RefreshCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        LocalDbTotalRecords = await _cccfDbContext.Products.CountAsync();
        await InitAsync();
    }

    [RelayCommand]
    private void SetPageFromAsLast()
    {
        PageFrom = QueryResponse?.TotalPages ?? 1;
    }

    [RelayCommand]
    private void ShowWindow()
    {
        Messenger.Send(new TrayMessage(), "ShowCccfScraperWindow");
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Reset(PageFrom);
        TaskbarItemProgressState = TaskbarItemProgressState.Normal;
        ProgressTimer.Start();

        int step = IsAscending ? 1 : -1;

        for (int i = PageFrom; IsAscending ? i <= PageTo : i >= PageTo; i += step)
        {
            while (true)
            {
                try
                {
                    CurrentPage = i;
                    await SyncDbAsync(CurrentPage, true, cancellationToken);
                    LocalDbTotalRecords = await _cccfDbContext.Products.CountAsync(cancellationToken);
                    int totalRecords = QueryResponse!.TotalRecords;

                    // 防止同步过程中 CCCF 服务器数据库的记录减少导致跳过某些页
                    if (totalRecords < _totalRecords)
                    {
                        int offset = (int)Math.Ceiling((double)(_totalRecords - totalRecords) / _pageSize);

                        if (PageTo >= PageFrom)
                        {
                            i = Math.Max(0, i - offset);
                        }
                        else
                        {
                            i = Math.Min(totalRecords, i + offset);
                        }
                    }

                    if (IsPageToLast)
                    {
                        PageTo = QueryResponse.TotalPages;
                    }

                    _totalRecords = totalRecords;
                    _syncedRecords += QueryResponse.Records.Count;
                    SyncedPages++;
                    ProgressTimer.TotalTasks = PageTo - PageFrom;
                    ProgressTimer.UpdateProgress(SyncedPages);

                    FireTaskStatus = FireTaskStatus.Normal;
                    TaskbarItemProgressState = TaskbarItemProgressState.Normal;

                    OnPropertyChanged(nameof(SyncStateToolTip));
                    OnPropertyChanged(nameof(TrayToolTip));
                    OnPropertyChanged(nameof(TrayShortToolTip));

                    await Task.Delay(Jitter.Next(Interval, 0.5), cancellationToken);

                    break;
                }
                catch (TaskCanceledException ex)
                {
                    FireTaskStatus = FireTaskStatus.Cancelled;
                    TaskbarItemProgressState = TaskbarItemProgressState.Paused;
                    PageFrom = CurrentPage;
                    CccfScraperLogs.Add(new CccfScraperLog(i, ex));

                    Growl.Info("已暂停同步。");

                    break;
                }
                catch (HttpRequestException ex)
                {
                    FireTaskStatus = FireTaskStatus.Error;
                    TaskbarItemProgressState = TaskbarItemProgressState.Error;
                    CccfScraperLogs.Add(new CccfScraperLog(i, ex));

                    HcMessageBox.Error($"HTTP 请求错误: {ex.GetHttpStatusDescription()}", App.AppName);
                }
                catch (RateLimiterRejectedException ex)
                {
                    FireTaskStatus = FireTaskStatus.Error;
                    TaskbarItemProgressState = TaskbarItemProgressState.Error;
                    CccfScraperLogs.Add(new CccfScraperLog(i, ex));

                    HcMessageBox.Error($"请求过于频繁。请稍后再试。(-{ex.RetryAfter?.TotalSeconds:F2}s)", App.AppName);
                }
                catch (TimeoutRejectedException ex)
                {
                    FireTaskStatus = FireTaskStatus.Error;
                    TaskbarItemProgressState = TaskbarItemProgressState.Error;
                    CccfScraperLogs.Add(new CccfScraperLog(i, ex));

                    HcMessageBox.Error($"请求超时: {ex.Message}", App.AppName);
                }
                catch (Exception ex)
                {
                    FireTaskStatus = FireTaskStatus.Error;
                    TaskbarItemProgressState = TaskbarItemProgressState.Error;
                    CccfScraperLogs.Add(new CccfScraperLog(i, ex));

                    HcMessageBox.Error($"同步发生错误。\n\n<{ex.GetType()}>\n{ex.Pretty()}", App.AppName);
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        ProgressTimer.Stop(true);
        FireTaskStatus = FireTaskStatus.Completed;
        TaskbarItemProgressState = TaskbarItemProgressState.None;

        if (ShutdownAfterCompletion)
        {
            _shutdownNotification = Notification.Show(App.Current.Services.GetRequiredService<ShutdownView>(), ShowAnimation.Fade, true);
        }
        else if (!cancellationToken.IsCancellationRequested)
        {
            Messenger.Send(new TrayMessage(), "ShowDbSyncCompletedBalloonTip");
        }
    }

    #endregion Commands

    #region OnPropertyChanged

    partial void OnFireTaskStatusChanged(FireTaskStatus value)
    {
        TaskbarItemProgressState = value switch {
            FireTaskStatus.Normal => TaskbarItemProgressState.Normal,
            FireTaskStatus.Completed => TaskbarItemProgressState.None,
            FireTaskStatus.Cancelled => TaskbarItemProgressState.Paused,
            FireTaskStatus.Error => TaskbarItemProgressState.Error,
            _ => TaskbarItemProgressState.None
        };
    }

    partial void OnIsPageToLastChanged(bool value)
    {
        if (value)
        {
            PageTo = QueryResponse?.TotalPages ?? 1;
        }
    }

    partial void OnPageFromChanged(int value)
    {
        CurrentPage = value;
    }

    #endregion OnPropertyChanged

    #region Methods

    private async Task InitAsync(CancellationToken cancellationToken = default)
    {
        Reset(CurrentPage);
        TaskbarItemProgressState = TaskbarItemProgressState.Indeterminate;

        try
        {
            await SyncDbAsync(1, false, cancellationToken);

            if (QueryResponse is null || QueryResponse.TotalPages < 1)
            {
                Growl.Info("初始化失败。未获取到数据。");
                CccfScraperLogs.Add(new CccfScraperLog(1, "初始化失败。未获取到数据。"));

                return;
            }

            _pageSize = QueryResponse.Records.Count;
            CurrentPage = Math.Max(1, _pageSize == 0 ? 0 : LocalDbTotalRecords / _pageSize);
            PageFrom = LocalDbTotalRecords >= QueryResponse.TotalRecords ? 1 : CurrentPage;

            if (CurrentPage > QueryResponse.TotalPages)
            {
                CurrentPage = 1;
            }

            PageTo = QueryResponse.TotalPages;
            IsPageToLast = true;
        }
        catch (TaskCanceledException ex)
        {
            Growl.Info("已暂停同步。");
            CccfScraperLogs.Add(new CccfScraperLog(1, ex));
        }
        catch (HttpRequestException ex)
        {
            CccfScraperLogs.Add(new CccfScraperLog(1, ex));
        }
        catch (Exception ex)
        {
            CccfScraperLogs.Add(new CccfScraperLog(1, ex));
        }
        finally
        {
            TaskbarItemProgressState = TaskbarItemProgressState.None;
            FireTaskStatus = FireTaskStatus.None;
        }
    }

    private void Reset(int currentPage)
    {
        FireTaskStatus = FireTaskStatus.Normal;
        TaskbarItemProgressState = TaskbarItemProgressState.None;
        CurrentPage = currentPage;
        SyncedPages = 0;
        _syncedRecords = 0;
        ProgressTimer.Reset();
    }

    private async Task SyncDbAsync(int page, bool upsert = true, CancellationToken cancellationToken = default)
    {
        CccfRequest.Page = page;

        QueryResponse = await _cccfService.QueryAsync(CccfRequest, cancellationToken);

        CccfScraperLogs.Add(new CccfScraperLog(page, "请求成功。"));

        if (upsert)
        {
            await _cccfDbContext.Products.UpsertRange(QueryResponse.Records).RunAsync(cancellationToken);
        }
    }

    #endregion Methods
}
