using System;
using System.Collections.Generic;
using System.Text;

using Firefly.Extensions;
using Firefly.Models.Responses;
using Firefly.Services.Navigation;
using Firefly.Services.Requests;

namespace Firefly.Models;

public class CccfQuerySession : NavigationSession<CccfQuerySession>
{
    public CccfRequest CombinedRequest { get; set; }

    public bool HasError { get; set; }

    public int PageNumber { get; set; }

    public QueryResponse<Cccf>? QueryResponse { get; set; }

    public string QueryResultMessage { get; set; }

    public double ScrolledVerticalOffset { get; set; }

    public CccfSmartRequest SmartRequest { get; set; }

    public bool UseCombinedQuery { get; set; }

    public CccfQuerySession(
        int pageNumber,
        CccfSmartRequest smartRequest,
        CccfRequest combinedRequest,
        bool useCombinedQuery,
        QueryResponse<Cccf>? queryResponse,
        bool hasError,
        string queryResultMessage,
        double scrolledVerticalOffset)
    {
        PageNumber = pageNumber;
        SmartRequest = smartRequest;
        CombinedRequest = combinedRequest;
        UseCombinedQuery = useCombinedQuery;
        QueryResponse = queryResponse;
        HasError = hasError;
        QueryResultMessage = queryResultMessage;
        ScrolledVerticalOffset = scrolledVerticalOffset;
    }

    public override CccfQuerySession DeepClone()
    {
        return new CccfQuerySession(
            PageNumber,
            SmartRequest.DeepClone(),
            CombinedRequest.DeepClone(),
            UseCombinedQuery,
            QueryResponse,
            HasError,
            QueryResultMessage,
            ScrolledVerticalOffset);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (UseCombinedQuery)
        {
            var combinedQueryStrs = new List<string>();

            if (!String.IsNullOrWhiteSpace(CombinedRequest.EnterpriseName))
            {
                combinedQueryStrs.Add($"企业名称: {CombinedRequest.EnterpriseName}");
            }

            if (!String.IsNullOrWhiteSpace(CombinedRequest.ProductName))
            {
                combinedQueryStrs.Add($"产品名称: {CombinedRequest.ProductName}");
            }

            if (!String.IsNullOrWhiteSpace(CombinedRequest.Model))
            {
                combinedQueryStrs.Add($"产品型号: {CombinedRequest.Model}");
            }

            if (!String.IsNullOrWhiteSpace(CombinedRequest.CertificateNo))
            {
                combinedQueryStrs.Add($"证书编号: {CombinedRequest.CertificateNo}");
            }

            if (!String.IsNullOrWhiteSpace(CombinedRequest.ReportNo))
            {
                combinedQueryStrs.Add($"检验报告: {CombinedRequest.ReportNo}");
            }

            if (!String.IsNullOrWhiteSpace(CombinedRequest.Status))
            {
                combinedQueryStrs.Add($"证书状态: {CombinedRequest.Status}");
            }

            if (CombinedRequest.CertDateStart is not null)
            {
                combinedQueryStrs.Add($"换(发)证日期: {CombinedRequest.CertDateStart}");
            }

            if (CombinedRequest.CertDateEnd is not null)
            {
                combinedQueryStrs.Add($"换(发)证日期(截止): {CombinedRequest.CertDateEnd}");
            }

            if (CombinedRequest.IssuedDateStart is not null)
            {
                combinedQueryStrs.Add($"报告签发日期: {CombinedRequest.IssuedDateStart}");
            }

            if (CombinedRequest.IssuedDateEnd is not null)
            {
                combinedQueryStrs.Add($"报告签发日期(截止): {CombinedRequest.IssuedDateEnd}");
            }

            if (!String.IsNullOrWhiteSpace(CombinedRequest.TestingCenter))
            {
                combinedQueryStrs.Add($"检验中心: {CombinedRequest.TestingCenter}");
            }

            if (combinedQueryStrs.Count > 0)
            {
                sb.AppendJoin(" | ", combinedQueryStrs);
            }
            else
            {
                sb.Append('*');
            }
        }
        else
        {
            if (!String.IsNullOrWhiteSpace(SmartRequest.Keyword))
            {
                sb.Append($"{SmartRequest.KeywordType.GetDescription()}: {SmartRequest.Keyword}");
            }
            else
            {
                sb.Append('*');
            }
        }

        sb.Append(" > ");
        sb.Append(QueryResponse?.ToString() ?? $"??? {{@{PageNumber}: {QueryResultMessage.Truncate(50)}}}");

        return sb.ToString();
    }
}
