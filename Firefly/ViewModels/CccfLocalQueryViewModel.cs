using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Firefly.Extensions;
using Firefly.Models;
using Firefly.Models.Responses;
using Firefly.Services.Abstractions;
using Firefly.Services.Requests;

using Microsoft.EntityFrameworkCore;

namespace Firefly.ViewModels;

public partial class CccfLocalQueryViewModel : CccfQueryViewModelBase
{
    #region Constructors

    public CccfLocalQueryViewModel(CccfDbContext cccfDbContext, INavigationService<CccfQuerySession> navigationService)
        : base(cccfDbContext, navigationService)
    {
        CanChangePageSize = true;
    }

    #endregion Constructors

    #region Methods

    public void Warmup()
    {
        // 执行伪查询进行 JIT 预热，避免首次查询时的性能问题
        _ = _cccfDbContext.Model;
    }

    protected override async Task<QueryResponse<Cccf>?> ExecuteQueryAsync(CancellationToken cancellationToken = default)
    {
        var queryable = _cccfDbContext.Products.AsQueryable();

        if (CccfRequest.IssuedDateStart is not null || CccfRequest.IssuedDateEnd is not null
            || !String.IsNullOrWhiteSpace(CccfRequest.TestingCenter))
        {
            QueryResultMessage = "本地查询不支持「报告签发日期」「报告签发日期(截止)」「检验中心」字段";

            return null;
        }

        queryable = queryable
            .FilterByLikeProperty(x => x.EnterpriseName, CccfRequest.EnterpriseName)
            .FilterByLikeProperty(x => x.ProductName, CccfRequest.ProductName)
            .FilterByLikeProperty(x => x.Models, CccfRequest.Model)
            .FilterByLikeProperty(x => x.CertificateNumber, CccfRequest.CertificateNo)
            .FilterByLikeProperty(x => x.ReportNumbers, CccfRequest.ReportNo)
            .FilterByLikeProperty(x => x.Status, CccfRequest.Status);

        if (CccfRequest.CertDateStart is not null)
        {
            queryable = queryable.Where(x => x.CertificateIssuedDate >= CccfRequest.CertDateStart);
        }

        if (CccfRequest.CertDateEnd is not null)
        {
            queryable = queryable.Where(x => x.CertificateIssuedDate <= CccfRequest.CertDateEnd);
        }

        int count = await queryable.CountAsync(cancellationToken);

        var rowIds = await queryable
            .OrderByDescending(x => x.CertificateIssuedDate)
            .ThenByDescending(x => x.CertificateExpirationDate)
            .Select(x => x.CertificateNumber)
            .Skip((int)PageSize * (PageNumber - 1))
            .Take((int)PageSize)
            .ToListAsync(cancellationToken);

        if (rowIds.Count == 0)
        {
            return QueryResponse<Cccf>.Empty;
        }

        // Join vs Where?
        // .Join(rowIds, c => c.CertificateNumber, certNo => certNo, (c, certNo) => c)
        var results = await queryable
            .Where(x => rowIds.Contains(x.CertificateNumber))
            .OrderByDescending(x => x.CertificateIssuedDate)
            .ThenByDescending(x => x.CertificateExpirationDate)
            .ToListAsync(cancellationToken);

        var resp = new QueryResponse<Cccf>(
            results,
            CccfRequest.Page,
            (int)Math.Ceiling(count / (double)PageSize),
            count);

        return resp;
    }

    #endregion Methods
}
