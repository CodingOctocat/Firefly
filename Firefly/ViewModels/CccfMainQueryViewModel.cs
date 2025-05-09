using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Firefly.Common;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Models.Requests;
using Firefly.Properties;
using Firefly.Services.Requests;

using Microsoft.Extensions.Configuration;

using HcMessageBox = HandyControl.Controls.MessageBox;

namespace Firefly.ViewModels;

public partial class CccfMainQueryViewModel : ObservableRecipient
{
    #region Fields

    private readonly CccfDbContext _cccfDbContext;

    private readonly IConfiguration _configuration;

    private readonly DispatcherTimer _debounceDispatcher = new();

    private readonly TaskCompletionSource<bool> _ensureCccfDbCreatedTcs = new();

    private double _localViewScrolledVerticalOffset;

    private double _onlineViewScrolledVerticalOffset;

    private CancellationTokenSource? _queryCts;

    #endregion Fields

    #region Properties

    public static string[] CccfCertificateStatuses { get; } = EnumHelper.GetDescriptions<CccfCertificateStatus>();

    public static string[] CccfTestingCenters { get; } = EnumHelper.GetDescriptions<CccfTestingCenter>();

    public static ComboBoxItemWrapper<PageSize>[] PageSizes { get; } = ComboBoxItemWrapper.CreateByValueDescription<PageSize>();

    public static ComboBoxItemWrapper<CccfFieldType>[] SmartRequestKeywordTypes { get; } = ComboBoxItemWrapper.CreateByValueDescription<CccfFieldType>(x => x != CccfFieldType.SmartMode);

    public bool CanCurrentModeQuery => (IsOnlineQueryEnabled && IsInOnlineQueryMode) || (IsLocalQueryEnabled && !IsInOnlineQueryMode);

    public bool CanEnableLocalQuery => IsLocalModeAllowed && IsOnlineQueryEnabled;

    /// <summary>
    /// 防止在切换在线/本地模式时触发 <see cref="PageUpdatedAsync"/>。
    /// </summary>
    public bool CanExecutePageUpdated { get; set; }

    public bool CanQuery => CccfOnlineQueryViewModel.CanQuery && (!IsLocalModeAllowed || CccfLocalQueryViewModel.CanQuery);

    public bool CanRealTimeQuery
    {
        get => field && Settings.Default.RealTimeQuery && !IsNavigating && !String.IsNullOrWhiteSpace(CccfQueryViewModel.SmartRequest.Keyword);
        private set;
    } = true;

    public CccfLocalQueryViewModel CccfLocalQueryViewModel { get; }

    public CccfOnlineQueryViewModel CccfOnlineQueryViewModel { get; }

    public CccfQueryViewModelBase CccfQueryViewModel => IsInOnlineQueryMode ? CccfOnlineQueryViewModel : CccfLocalQueryViewModel;

    public FindInPageBarViewModel FindInPageBarViewModel { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CccfQueryViewModel))]
    [NotifyPropertyChangedFor(nameof(CanCurrentModeQuery))]
    public partial bool IsInOnlineQueryMode { get; set; } = true;

    public bool IsLocalModeAllowed { get; }

    public bool IsLocalQueryEnabled
    {
        get => IsLocalModeAllowed && field;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(CanCurrentModeQuery));
                OnPropertyChanged(nameof(LocalQueryTabToolTip));

                if (!field)
                {
                    IsInOnlineQueryMode = true;
                }
            }
        }
    } = true;

    public bool IsNavigating => CccfOnlineQueryViewModel.IsNavigating || (IsLocalModeAllowed && CccfLocalQueryViewModel.IsNavigating);

    public bool IsOnlineQueryEnabled
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(CanCurrentModeQuery));
                OnPropertyChanged(nameof(CanEnableLocalQuery));
                OnPropertyChanged(nameof(OnlineQueryTabToolTip));

                if (!field)
                {
                    IsInOnlineQueryMode = false;
                }
            }
        }
    } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleMoreQueryToolTip))]
    public partial bool IsOpenMoreQueryConditions { get; set; }

    public string? LocalModeDisallowedToolTip => IsLocalModeAllowed ? null : "\n\n* 暂未对普通用户开放";

    public string? LocalQueryTabToolTip => IsLocalQueryEnabled ? "Alt+2" : $"未启用本地查询功能{LocalModeDisallowedToolTip}";

    public string LocalQueryToolTip => $"启用本地数据库查询 (如果可用){LocalModeDisallowedToolTip}";

    public string? OnlineQueryTabToolTip => IsOnlineQueryEnabled ? "Alt+1" : "未启用在线查询功能";

    public string ToggleCombinedQueryToolTip => UseCombinedQuery ? "收起组合查询" : "展开组合查询";

    public string ToggleMoreQueryToolTip => IsOpenMoreQueryConditions ? "隐藏更多查询条件" : "显示更多查询条件";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleCombinedQueryToolTip))]
    public partial bool UseCombinedQuery { get; set; }

    #endregion Properties

    #region Constructors & Recipients

    public CccfMainQueryViewModel(CccfOnlineQueryViewModel cccfOnlineQueryViewModel, CccfLocalQueryViewModel cccfLocalQueryViewModel, CccfDbContext cccfDbContext, IConfiguration configuration)
    {
        IsActive = true;

        CccfOnlineQueryViewModel = cccfOnlineQueryViewModel;
        CccfLocalQueryViewModel = cccfLocalQueryViewModel;
        _cccfDbContext = cccfDbContext;
        _configuration = configuration;
        IsLocalModeAllowed = _configuration.GetValue("Cccf:LocalMode", false);

        CccfOnlineQueryViewModel.PropertyChanged += CccfQueryViewModelPropertyChanged;

        if (IsLocalModeAllowed)
        {
            CccfLocalQueryViewModel.PropertyChanged += CccfQueryViewModelPropertyChanged;
        }
    }

    public CccfMainQueryViewModel(CccfOnlineQueryViewModel cccfOnlineQueryViewModel, CccfDbContext cccfDbContext, IConfiguration configuration)
        : this(cccfOnlineQueryViewModel, null!, cccfDbContext, configuration)
    { }

    protected override void OnActivated()
    {
        base.OnActivated();

        Messenger.Register<FocusOnQueryBarMessage>(this,
            (r, m) => FocusOnQueryBar(m.NeedsSelectAll, m.IsRestore));

        Messenger.Register<ManualQueryAsyncRequestMessage>(this,
            (r, m) => {
                _queryCts?.Cancel();
                _queryCts = new();
                m.Reply(ManualQueryAsyncRequestMessageHandler(m.Request, _queryCts.Token));
            });
    }

    #endregion Constructors & Recipients

    #region Commands

    [RelayCommand]
    private static void ShowWildcardCharacterTips()
    {
        HcMessageBox.Show(
            """
            = 精确匹配

            ? 任何单个字符

            _ 任何单个字符 (仅限本地查询)

            * 包含零个或多个字符的任意字符串

            % 包含零个或多个字符的任意字符串 (仅限本地查询)

            \ 转义字符

            本地查询默认模糊匹配。在线查询仅部分字段支持通配符。
            """,
            "通配符");
    }

    [RelayCommand]
    private void ClearQueryFields()
    {
        if (UseCombinedQuery)
        {
            CccfOnlineQueryViewModel.CombinedRequest.Clear();
        }
        else
        {
            CccfOnlineQueryViewModel.SmartRequest.Clear();
        }

        if (IsLocalModeAllowed)
        {
            if (UseCombinedQuery)
            {
                CccfLocalQueryViewModel.CombinedRequest.Clear();
            }
            else
            {
                CccfLocalQueryViewModel.CccfRequest.Clear();
            }
        }
    }

    [RelayCommand]
    private void EscShortcut()
    {
        if (CccfQueryViewModel.QueryCommand.CanBeCanceled)
        {
            CccfQueryViewModel.QueryCancelCommand.Execute(null);
        }
        else if (FindInPageBarViewModel.IsFindEnabled)
        {
            FindInPageBarViewModel.DisableFindCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadedAsync()
    {
        await CccfCatalogyResources.ReadAsync();

        if (IsLocalModeAllowed)
        {
            // TODO: EnsureCreatedAsync 不使用迁移来创建数据库。此外，创建的数据库以后也不能使用迁移功能进行更新。
            // HACK: 如果调用 CccfLocalQueryViewModel.EnsureCreatedAsync()，会引发 InvalidOperationException(Microsoft.EntityFrameworkCore) 异常
            // An attempt was made to use the model while it was being created.
            // A DbContext instance cannot be used inside 'OnModelCreating' in any way that makes use of the model that is being created.
            await _cccfDbContext.Database.EnsureCreatedAsync();
            CccfLocalQueryViewModel.Warmup();
            _ensureCccfDbCreatedTcs.TrySetResult(true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    private async Task NavigateFirstPageAsync(CancellationToken cancellationToken = default)
    {
        if (CccfQueryViewModel.QueryResponse is null)
        {
            return;
        }

        CccfQueryViewModel.PageNumber = 1;
        CanExecutePageUpdated = true;
        await PageUpdatedCommand.ExecuteAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    private async Task NavigateLastPageAsync(CancellationToken cancellationToken = default)
    {
        if (CccfQueryViewModel.QueryResponse is null)
        {
            return;
        }

        CccfQueryViewModel.PageNumber = CccfQueryViewModel.QueryResponse.TotalPages;
        CanExecutePageUpdated = true;
        await PageUpdatedCommand.ExecuteAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    private async Task NavigateNextPageAsync(CancellationToken cancellationToken = default)
    {
        if (CccfQueryViewModel.PageNumber == CccfQueryViewModel.QueryResponse?.TotalPages)
        {
            return;
        }

        CccfQueryViewModel.PageNumber++;
        CanExecutePageUpdated = true;
        await PageUpdatedCommand.ExecuteAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    private async Task NavigatePreviousPageAsync(CancellationToken cancellationToken = default)
    {
        if (CccfQueryViewModel.PageNumber == 0)
        {
            return;
        }

        CccfQueryViewModel.PageNumber--;
        CanExecutePageUpdated = true;
        await PageUpdatedCommand.ExecuteAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    private async Task NavigateToQueryAsync(CancellationToken cancellationToken = default)
    {
        RealTimeNavigateToQueryCancelCommand.Execute(null);

        SyncQueryRequest(IsInOnlineQueryMode);

        var queryTasks = new List<Task>();

        if (IsOnlineQueryEnabled)
        {
            var onlineTask = CccfOnlineQueryViewModel.NavigateToQueryCommand.ExecuteAsync(cancellationToken);

            queryTasks.Add(onlineTask);
        }

        if (IsLocalModeAllowed && IsLocalQueryEnabled)
        {
            var localTask = CccfLocalQueryViewModel.NavigateToQueryCommand.ExecuteAsync(cancellationToken);

            queryTasks.Add(localTask);
        }

        await WhenAllThenThrow(queryTasks);

        FocusOnQueryBar();
    }

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    private async Task PageUpdatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (CanExecutePageUpdated)
            {
                await CccfQueryViewModel.PageUpdatedCommand.ExecuteAsync(cancellationToken);
            }
        }
        finally
        {
            CanExecutePageUpdated = false;
        }
    }

    [RelayCommand]
    private void PredictKeywordType()
    {
        if (CccfQueryViewModel.SmartRequest.KeywordType == CccfFieldType.SmartMode)
        {
            CccfQueryViewModel.SmartRequest.Predict();
        }
    }

    [RelayCommand(CanExecute = nameof(CanRealTimeQuery), IncludeCancelCommand = true)]
    private async Task RealTimeNavigateToQueryAsync(CancellationToken cancellationToken = default)
    {
        await _debounceDispatcher.DebounceAsync(
            () => {
                // 这里需要额外判空，否则以下情况仍将触发：
                // "    aaa  bbb" ---> "    "
                if (String.IsNullOrWhiteSpace(CccfQueryViewModel.SmartRequest.Keyword))
                {
                    return Task.CompletedTask;
                }

                return NavigateToQueryCommand.ExecuteAsync(cancellationToken);
            },
            TimeSpan.FromSeconds(0.5),
            cancellationToken: cancellationToken);
    }

    [RelayCommand]
    private void ToggleQueryModePage(bool onlineMode)
    {
        if (!onlineMode && !IsLocalModeAllowed)
        {
            return;
        }

        IsInOnlineQueryMode = onlineMode;
    }

    #endregion Commands

    #region OnPropertyChanged

    partial void OnIsInOnlineQueryModeChanged(bool value)
    {
        if (value)
        {
            // 切换到在线视图后，恢复在线视图滚动条位置
            Messenger.Send(new ScrollBarOffsetMessage() {
                VerticalOffset = _onlineViewScrolledVerticalOffset
            }, "RestoreCccfQueryResultsScrollOffset");
        }
        else
        {
            // 切换到本地视图后，恢复本地视图滚动条位置
            Messenger.Send(new ScrollBarOffsetMessage() {
                VerticalOffset = _localViewScrolledVerticalOffset
            }, "RestoreCccfQueryResultsScrollOffset");
        }
    }

    partial void OnIsInOnlineQueryModeChanging(bool value)
    {
        if (value)
        {
            // 即将切换为在线视图时，保存本地视图滚动条位置
            _localViewScrolledVerticalOffset = Messenger.Send<RequestMessage<ScrollBarOffsetMessage>, string>(
                "RequestCccfQueryResultsScrollOffset").Response.VerticalOffset;
        }
        else
        {
            // 即将切换为本地视图时，保存在线视图滚动条位置
            _onlineViewScrolledVerticalOffset = Messenger.Send<RequestMessage<ScrollBarOffsetMessage>, string>(
                "RequestCccfQueryResultsScrollOffset").Response.VerticalOffset;
        }
    }

    partial void OnUseCombinedQueryChanged(bool value)
    {
        if (IsLocalModeAllowed)
        {
            CccfLocalQueryViewModel.UseCombinedQuery = value;
        }

        CccfOnlineQueryViewModel.UseCombinedQuery = value;
    }

    #endregion OnPropertyChanged

    #region Methods

    public void FocusOnQueryBar(bool needsSelectAll = false, bool isRestore = true)
    {
        if (UseCombinedQuery)
        {
            Messenger.Send(new FocusOnCombinedQueryBarMessage(needsSelectAll, isRestore));
        }
        else
        {
            Messenger.Send(new FocusOnSmartQueryBarMessage(needsSelectAll));
        }
    }

    private static async Task WhenAllThenThrow(IEnumerable<Task> tasks)
    {
        var allTasks = tasks.ToList();

        try
        {
            await Task.WhenAll(allTasks).ConfigureAwait(false);
        }
        catch
        { }

        var exceptions = allTasks
            .Where(t => t.IsFaulted)
            .SelectMany(t => t.Exception?.InnerExceptions ?? Enumerable.Empty<Exception>());

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }

    private async Task ManualQueryAsyncRequestMessageHandler(ICccfRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return;
        }

        if (request is CccfSmartRequest cccfSmartRequest)
        {
            if (String.IsNullOrWhiteSpace(cccfSmartRequest.Keyword))
            {
                return;
            }

            var keywordType = cccfSmartRequest.KeywordType;
            IsInOnlineQueryMode = true;
            UseCombinedQuery = false;
            CanRealTimeQuery = false;
            // 赋值后将触发数据绑定重新计算字段类型，需要恢复它
            CccfOnlineQueryViewModel.SmartRequest = cccfSmartRequest;

            if (keywordType != CccfFieldType.SmartMode)
            {
                CccfOnlineQueryViewModel.SmartRequest.KeywordType = keywordType;
            }

            SyncQueryRequest(true);
            CanRealTimeQuery = true;
        }
        else
        {
            // 注意调用的先后顺序
            // 错误的调用顺序：
            //IsInOnlineQueryMode = true;
            //CccfOnlineQueryViewModel.CombinedRequest = message.CccfRequest;
            // 这里会导致 CccfOnlineQueryViewModel.CombinedRequest 丢失，详见 CccfQueryViewModelBase.OnUseCombinedQueryChanged()
            //UseCombinedQuery = true;

            // 正确的调用顺序：
            IsInOnlineQueryMode = true;
            UseCombinedQuery = true;
            CccfOnlineQueryViewModel.CombinedRequest = request.AsCccfRequest();
            SyncQueryRequest(true);
        }

        await NavigateToQueryAsync(cancellationToken);
    }

    private void SyncQueryRequest(bool inOnlineQuery)
    {
        if (!IsLocalModeAllowed)
        {
            return;
        }

        if (inOnlineQuery)
        {
            CccfLocalQueryViewModel.SmartRequest = CccfOnlineQueryViewModel.SmartRequest;
            CccfLocalQueryViewModel.CombinedRequest = CccfOnlineQueryViewModel.CombinedRequest;
        }
        else
        {
            CccfOnlineQueryViewModel.SmartRequest = CccfLocalQueryViewModel.SmartRequest;
            CccfOnlineQueryViewModel.CombinedRequest = CccfLocalQueryViewModel.CombinedRequest;
        }
    }

    #endregion Methods

    #region Event Handlers

    private void CccfQueryViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CccfQueryViewModelBase.IsNavigating))
        {
            OnPropertyChanged(nameof(IsNavigating));
        }

        if (e.PropertyName == nameof(CccfQueryViewModelBase.CanQuery))
        {
            OnPropertyChanged(nameof(CanQuery));
            NavigateToQueryCommand.NotifyCanExecuteChanged();
            FindInPageBarViewModel.CanExecuteFindCommand = CanQuery;
        }
    }

    #endregion Event Handlers
}
