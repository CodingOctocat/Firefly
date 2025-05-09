using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Firefly.Models;
using Firefly.Models.Messages;
using Firefly.Models.Responses;
using Firefly.Services.Abstractions;
using Firefly.Services.Navigation;
using Firefly.Services.Requests;

using HandyControl.Controls;

namespace Firefly.ViewModels;

public abstract partial class CccfQueryViewModelBase : ObservableRecipient
{
    #region Fields

    protected readonly CccfDbContext _cccfDbContext;

    protected readonly INavigationService<CccfQuerySession> _navigationService;

    protected readonly Stopwatch _stopwatch = new();

    protected CccfRequest? _lastCombinedRequest;

    protected int _lastPageNumberBeforeCancel = 1;

    protected bool _pageNumberChanged;

    protected bool _pageSizeChanged;

    protected double _scrolledVerticalOffset;

    #endregion Fields

    #region Properties

    [ObservableProperty]
    public partial IEnumerable<CccfQuerySession>? BackStackList { get; protected set; }

    [ObservableProperty]
    public partial bool CanChangePageSize { get; protected set; }

    public bool CanGoBack => _navigationService.CanGoBack && !IsNavigating;

    public bool CanGoForward => _navigationService.CanGoForward && !IsNavigating;

    public bool CanQuery => !IsNavigating;

    public bool CanRefresh => CanQuery;

    public CccfRequest CccfRequest => UseCombinedQuery ? CombinedRequest : SmartRequest;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CccfRequest))]
    [NotifyPropertyChangedRecipients]
    public partial CccfRequest CombinedRequest { get; set; } = new(1);

    [ObservableProperty]
    public partial IEnumerable<CccfQuerySession>? ForwardStackList { get; protected set; }

    [ObservableProperty]
    public partial bool HasError { get; protected set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBackingOrForwarding))]
    public partial bool IsBacking { get; protected set; }

    public bool IsBackingOrForwarding => IsBacking || IsForwarding;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBackingOrForwarding))]
    public partial bool IsForwarding { get; protected set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanQuery))]
    [NotifyCanExecuteChangedFor(nameof(NavigateToQueryCommand))]
    [NotifyCanExecuteChangedFor(nameof(PageUpdatedCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoForwardCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    public partial bool IsNavigating { get; protected set; }

    [ObservableProperty]
    public partial int PageNumber { get; set; } = 1;

    [ObservableProperty]
    public partial PageSize PageSize { get; set; } = PageSize.Ten;

    /// <summary>
    /// 查询发生异常时为 <see langword="null"/>。
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRefresh))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    public partial QueryResponse<Cccf>? QueryResponse { get; protected set; }

    [ObservableProperty]
    public partial string QueryResultMessage { get; protected set; } = "暂无记录";

    [ObservableProperty]
    public partial TimeSpan QueryTime { get; protected set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CccfRequest))]
    [NotifyPropertyChangedRecipients]
    public partial CccfSmartRequest SmartRequest { get; set; } = new(1);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CccfRequest))]
    public partial bool UseCombinedQuery { get; set; }

    #endregion Properties

    #region Constructors & Recipients

    protected CccfQueryViewModelBase(CccfDbContext cccfDbContext, INavigationService<CccfQuerySession> navigationService)
    {
        _cccfDbContext = cccfDbContext;
        _navigationService = navigationService;

        _navigationService.IsNavigatingChanged += NavigationService_IsNavigatingChanged;
        _navigationService.Navigating += NavigationService_Navigating;
        _navigationService.Navigated += NavigationService_Navigated;
    }

    #endregion Constructors & Recipients

    #region Commands

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    public virtual async Task ChangePageSizeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_pageSizeChanged)
            {
                await NewQueryAsync(cancellationToken: cancellationToken);
            }
        }
        finally
        {
            _pageSizeChanged = false;
        }
    }

    [RelayCommand]
    public virtual void ClearQueryFields()
    {
        SmartRequest.Clear();
        CccfRequest.Clear();
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public virtual void GoBack(CccfQuerySession? session = null)
    {
        IsBacking = true;

        if (session is null)
        {
            _navigationService.GoBack();
        }
        else
        {
            _navigationService.GoBackTo(session);
        }

        IsBacking = false;
    }

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    public virtual void GoForward(CccfQuerySession? session = null)
    {
        IsForwarding = true;

        if (session is null)
        {
            _navigationService.GoForward();
        }
        else
        {
            _navigationService.GoForwardTo(session);
        }

        IsForwarding = false;
    }

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    public virtual async Task NavigateToQuery(CancellationToken cancellationToken = default)
    {
        await NewQueryAsync(1, cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanQuery), IncludeCancelCommand = true)]
    public virtual async Task PageUpdatedAsync(CancellationToken cancellationToken = default)
    {
        await NewQueryAsync(cancellationToken: cancellationToken);
    }

    [RelayCommand(IncludeCancelCommand = true)]
    public virtual async Task QueryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _lastPageNumberBeforeCancel = PageNumber;
            HasError = false;
            QueryResultMessage = "";

            await BeforeExecuteQueryAsync(cancellationToken);

            _stopwatch.Restart();

            QueryResponse = await Task.Run(async () => await ExecuteQueryAsync(cancellationToken), cancellationToken);

            _stopwatch.Stop();
            QueryTime = _stopwatch.Elapsed;

            if (HasError)
            {
                return;
            }

            if (QueryResponse?.HasRecord is not true)
            {
                QueryResultMessage = "暂无记录";
            }

            try
            {
                await AfterExecuteQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new AfterExecuteQueryException(ex.Message, ex.InnerException);
            }
        }
        catch (AfterExecuteQueryException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            HasError = true;
            QueryResponse = null;
            QueryResultMessage = "查询请求被取消。";
            Growl.Info(QueryResultMessage);

            throw;
        }
        catch (Exception ex)
        {
            HasError = true;
            QueryResponse = null;

            if (String.IsNullOrWhiteSpace(QueryResultMessage))
            {
                QueryResultMessage = ex.Message;
            }

            throw;
        }
        finally
        {
            SmartRequest.AcceptChanges();
            CccfRequest.AcceptChanges();
            IsNavigating = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefresh), IncludeCancelCommand = true)]
    public virtual async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await _navigationService.NavigateAsync(async () => {
            SmartRequest.RejectChanges();
            CccfRequest.RejectChanges();

            CccfQuerySession session;
            PageNumber = _lastPageNumberBeforeCancel;

            try
            {
                await QueryCommand.ExecuteAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                session = new CccfQuerySession(
                    _lastPageNumberBeforeCancel,
                    SmartRequest,
                    CombinedRequest,
                    UseCombinedQuery,
                    QueryResponse,
                    HasError,
                    "刷新请求被取消。",
                    _scrolledVerticalOffset);

                return session;
            }

            session = new CccfQuerySession(
                PageNumber,
                SmartRequest,
                CombinedRequest,
                UseCombinedQuery,
                QueryResponse,
                HasError,
                QueryResultMessage,
                _scrolledVerticalOffset);

            return session;
        }, true);
    }

    [RelayCommand]
    public virtual void SmartSearchTextChanged()
    {
        SmartRequest.KeywordType = CccfFieldClassifier.Predict(SmartRequest.Keyword);

        _lastCombinedRequest = null;
    }

    #endregion Commands

    #region OnPropertyChanged

    partial void OnPageNumberChanged(int value)
    {
        _pageNumberChanged = true;

        if (UseCombinedQuery)
        {
            CombinedRequest.Page = value;
        }
        else
        {
            SmartRequest.Page = value;
        }
    }

    partial void OnPageSizeChanged(PageSize value)
    {
        _pageSizeChanged = true;
    }

    partial void OnUseCombinedQueryChanged(bool value)
    {
        if (value)
        {
            if (_lastCombinedRequest is null)
            {
                CombinedRequest = SmartRequest.AsCccfRequest();
            }
            else
            {
                CombinedRequest = _lastCombinedRequest;
            }
        }
        else
        {
            _lastCombinedRequest = CombinedRequest;
        }
    }

    #endregion OnPropertyChanged

    #region Methods

    public virtual async Task NewQueryAsync(int pageNumber = 0, CancellationToken cancellationToken = default)
    {
        await _navigationService.NavigateAsync(async () => {
            if (pageNumber > 0)
            {
                PageNumber = pageNumber;
            }

            var session = new CccfQuerySession(
                PageNumber,
                SmartRequest,
                CombinedRequest,
                UseCombinedQuery,
                QueryResponse,
                HasError,
                QueryResultMessage,
                0d);

            try
            {
                await QueryCommand.ExecuteAsync(cancellationToken);
                session.QueryResponse = QueryResponse;
            }
            catch (TaskCanceledException)
            {
                session.QueryResponse = null;
                session.QueryResultMessage = "查询请求被取消。";
            }

            Messenger.Send(new ScrollBarOffsetMessage() {
                VerticalOffset = 0d
            }, "RestoreCccfQueryResultsScrollOffset");

            return session;
        }, false);
    }

    protected virtual Task AfterExecuteQueryAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected virtual Task BeforeExecuteQueryAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected abstract Task<QueryResponse<Cccf>?> ExecuteQueryAsync(CancellationToken cancellationToken = default);

    protected void RaiseNavigationBarStatus()
    {
        OnPropertyChanged(nameof(CanGoBack));
        GoBackCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanGoForward));
        GoForwardCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanRefresh));
        RefreshCommand.NotifyCanExecuteChanged();
    }

    #endregion Methods

    #region Event Handlers

    protected virtual void NavigationService_IsNavigatingChanged(object? sender, bool e)
    {
        IsNavigating = e;
    }

    protected virtual void NavigationService_Navigated(object? sender, NavigationEventArgs<CccfQuerySession> e)
    {
        if (e.NavigationMode is NavigationMode.Back or NavigationMode.Forward)
        {
            var cloned = e.Current.DeepClone();
            _lastPageNumberBeforeCancel = cloned.PageNumber;
            PageNumber = cloned.PageNumber;
            SmartRequest = cloned.SmartRequest;

            // 必须先设置 UseCombinedQuery，否则 OnUseCombinedQueryChanged() 将修改 CombinedRequest
            UseCombinedQuery = cloned.UseCombinedQuery;
            CombinedRequest = cloned.CombinedRequest;
            QueryResponse = cloned.QueryResponse;
            HasError = cloned.HasError;
            QueryResultMessage = cloned.QueryResultMessage;

            double scrolledVerticalOffset = cloned.ScrolledVerticalOffset;

            Messenger.Send(new ScrollBarOffsetMessage() {
                VerticalOffset = scrolledVerticalOffset
            }, "RestoreCccfQueryResultsScrollOffset");
        }

        BackStackList = e.BackStack.Reverse();
        ForwardStackList = e.ForwardStack.Reverse();

        RaiseNavigationBarStatus();
    }

    protected virtual void NavigationService_Navigating(object? sender, NavigationEventArgs<CccfQuerySession> e)
    {
        RaiseNavigationBarStatus();

        // 首次导航时 Current 为 null
        if (e.Current is not null)
        {
            _scrolledVerticalOffset = Messenger.Send<RequestMessage<ScrollBarOffsetMessage>, string>(
                 "RequestCccfQueryResultsScrollOffset").Response.VerticalOffset;

            e.Current.ScrolledVerticalOffset = _scrolledVerticalOffset;
        }
    }

    #endregion Event Handlers
}
