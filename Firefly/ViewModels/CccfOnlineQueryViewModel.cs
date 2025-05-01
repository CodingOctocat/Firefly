using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Firefly.Extensions;
using Firefly.Factories;
using Firefly.Models;
using Firefly.Models.Responses;
using Firefly.Services.Abstractions;

using HandyControl.Controls;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Polly.RateLimiting;
using Polly.Timeout;

namespace Firefly.ViewModels;

public partial class CccfOnlineQueryViewModel : CccfQueryViewModelBase
{
    #region Fields

    private readonly ICccfService _cccfService;

    private readonly IConfiguration _configuration;

    private readonly bool _isLocalQueryAllowed;

    #endregion Fields

    #region Constructors

    public CccfOnlineQueryViewModel(ICccfServiceFactory cccfServiceFactory, CccfDbContext cccfDbContext, INavigationService<CccfQuerySession> navigationService, IConfiguration configuration)
        : base(cccfDbContext, navigationService)
    {
        _cccfService = cccfServiceFactory.Create(CccfServiceFactory.ManualPipeline);
        _configuration = configuration;
        _isLocalQueryAllowed = _configuration.GetValue("Cccf:LocalMode", false);
    }

    #endregion Constructors

    #region Methods

    protected override async Task AfterExecuteQueryAsync(CancellationToken cancellationToken = default)
    {
        if (_isLocalQueryAllowed)
        {
            if (QueryResponse is not null && QueryResponse.HasRecord)
            {
                await _cccfDbContext.Products.UpsertRange(QueryResponse.Records).RunAsync(cancellationToken);
            }
        }
    }

    protected override async Task<QueryResponse<Cccf>?> ExecuteQueryAsync(CancellationToken cancellationToken = default)
    {
        QueryResponse<Cccf>? resp = null;

        try
        {
            resp = await _cccfService.QueryAsync(CccfRequest, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            HasError = true;
            QueryResultMessage = $"HTTP 请求错误: {ex.GetHttpStatusDescription()}";
            Growl.Error(QueryResultMessage);
        }
        catch (RateLimiterRejectedException ex)
        {
            HasError = true;
            QueryResultMessage = "请求过于频繁。请稍后再试。";
            Growl.Info($"{QueryResultMessage}(-{ex.RetryAfter?.TotalSeconds:F2}s)");
        }
        catch (TimeoutRejectedException ex)
        {
            HasError = true;
            QueryResultMessage = $"请求超时: {ex.Message}";
            Growl.Error(QueryResultMessage);
        }

        return resp;
    }

    #endregion Methods
}
