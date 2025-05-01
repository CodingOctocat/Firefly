using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Firefly.Factories;
using Firefly.Models.Responses;
using Firefly.Services.Abstractions;
using Firefly.Services.Requests;

using Polly.RateLimiting;

namespace Firefly.Models;

public partial class FireCheckContext : ObservableObject, IPageFindable<FindableText>
{
    private static int _order = 1;

    private readonly ICccfService _cccfService;

    private readonly IFireChecker<Cccf> _fireChecker;

    public Cccf? Cccf => CccfQueryResponse?.Records?.Count > 0 ? CccfQueryResponse.Records[0] : null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Cccf))]
    public partial QueryResponse<Cccf>? CccfQueryResponse { get; private set; }

    [ObservableProperty]
    public partial CccfRequest? CccfRequest { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsSkipped))]
    [NotifyPropertyChangedFor(nameof(IsRuleSkipped))]
    [NotifyPropertyChangedFor(nameof(IsContentSkipped))]
    public partial FireErrors FireErrors { get; private set; }

    public FireProduct FireProduct { get; }

    public string FireSystem => FireProduct.FireSystem;

    public int FireSystemOrder => FireProduct.FireSystem.Order;

    public bool? HasError => IsChecked ? FireErrors is not (FireErrors.Unknown or FireErrors.RuleSkipped or FireErrors.ContentSkipped or FireErrors.None) : null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial bool IsChecked { get; private set; }

    public bool IsContentSkipped => FireErrors == FireErrors.ContentSkipped;

    public bool IsRuleSkipped => FireErrors == FireErrors.RuleSkipped;

    public bool IsSkipped => IsRuleSkipped || IsContentSkipped;

    public int Order { get; }

    public FireCheckContext(FireProduct fireProduct, ICccfServiceFactory cccfServiceFactory, IFireChecker<Cccf> fireChecker)
    {
        FireProduct = fireProduct;
        _cccfService = cccfServiceFactory.Create(CccfServiceFactory.AutoPipeline);
        _fireChecker = fireChecker;
        Order = _order++;
    }

    public static void ResetOrder()
    {
        _order = 1;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        Reset();

        try
        {
            if (_fireChecker.PreCheck(FireProduct, out var fireErrors))
            {
                FireErrors = fireErrors;

                return;
            }

            var requests = CreateRequests();

            if (requests.Count > 0)
            {
                await QueryByPriorityAsync(requests, cancellationToken);
            }

            FireErrors = _fireChecker.Check(FireProduct, Cccf);
        }
        finally
        {
            IsChecked = true;
        }
    }

    public void Reset()
    {
        FireErrors = FireErrors.None;
        CccfQueryResponse = null;
        IsChecked = false;
    }

    private List<CccfRequest> CreateRequests()
    {
        var requests = new List<CccfRequest>();

        if (FireProduct.CertificateNumber.Text is not ("" or "/"))
        {
            requests.Add(new CccfRequest(1) { CertificateNo = FireProduct.CertificateNumber.Text });
        }

        if (FireProduct.ReportNumber?.Text is not (null or "" or "/"))
        {
            requests.Add(new CccfRequest(1) { ReportNo = FireProduct.ReportNumber.Text });
        }

        if (FireProduct.Model.Text is not ("" or "/"))
        {
            requests.Add(new CccfRequest(1) { Model = FireProduct.Model.Text });
        }

        return requests;
    }

    private async Task QueryByPriorityAsync(List<CccfRequest> requests, CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            try
            {
                CccfRequest = request;
                CccfQueryResponse = await _cccfService.QueryAsync(request, cancellationToken);

                if (CccfQueryResponse?.TotalRecords > 0)
                {
                    return;
                }
            }
            catch (Exception ex) when (ex is not (TaskCanceledException or RateLimiterRejectedException))
            { }
        }
    }

    #region IPageFindable<FindableText>

    public IEnumerable<FindableText> GetFindableInfos()
    {
        return Cccf?.GetFindableInfos() ?? [];
    }

    #endregion IPageFindable<FindableText>
}
