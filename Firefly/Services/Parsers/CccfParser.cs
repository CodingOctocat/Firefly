using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using AngleSharp;

using Firefly.Extensions;
using Firefly.Models.Responses;
using Firefly.Services.Abstractions;

namespace Firefly.Services.Parsers;

public partial class CccfParser : IResponseParser<QueryResponse<Cccf>>
{
    #region Selectors

    private const string CategorySelector = "td:nth-child(1) > div > div:nth-child(1)";

    private const string CertExpirationSelector = "td:nth-child(4)";

    private const string CertIssuedSelector = "td:nth-child(3)";

    private const string CertNoSelector = "td:nth-child(2) > div:nth-child(1) > a";

    private const string EnterpriseSelector = "td:nth-child(1) > div > div:nth-child(3) > div:nth-child(4)";

    private const string ModelsSelector = "td:nth-child(1) > div > div:nth-child(3) > div:nth-child(2)";

    private const string NameSelector = "td:nth-child(1) > div > div:nth-child(2) > a";

    private const string ReportNosSelector = "td:nth-child(2) > div:nth-child(2) > span";

    private const string StatusSelector = "td:nth-child(5)";

    private const string TableRowsSelector = "#pageForm > table > tbody > tr > td > table > tbody > tr > td > table > tbody > tr";

    #endregion Selectors

    public async Task<QueryResponse<Cccf>> ParseAsync(string text)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(text));
        var rowElements = document.QuerySelectorAll(TableRowsSelector);

        if (rowElements.Length <= 1)
        {
            return QueryResponse<Cccf>.Empty;
        }

        var records = new List<Cccf>();

        foreach (var elem in rowElements.SkipLast(1))
        {
            string category = elem.GetTextContentBySelectors(CategorySelector);
            category = category.RemovePrefix("产品类别 ：");

            string name = elem.GetTextContentBySelectors(NameSelector);

            string[] models = elem.GetInnerTextBySelectors(ModelsSelector)
                .Split("<br>", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string ent = elem.GetTextContentBySelectors(EnterpriseSelector);
            ent = ent.RemovePrefix("认证委托人 ：");

            string certNo = elem.GetTextContentBySelectors(CertNoSelector);

            string[] reportNos = elem.GetInnerTextBySelectors(ReportNosSelector)
                .Split("<br>", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string certIssued = elem.GetTextContentBySelectors(CertIssuedSelector);
            certIssued = DateRegex().Match(certIssued).Value;

            string certExpiration = elem.GetTextContentBySelectors(CertExpirationSelector);
            certExpiration = DateRegex().Match(certExpiration).Value;

            string status = elem.GetTextContentBySelectors(StatusSelector);

            var cccf = new Cccf(
                category,
                name,
                [.. models],
                ent,
                certNo,
                [.. reportNos],
                DateOnly.Parse(certIssued),
                DateOnly.Parse(certExpiration),
                status.CleanText());

            records.Add(cccf);
        }

        string paginationInfo = rowElements[^1].TextContent.CleanText(h: true, v: true);
        var match = PaginationInfoRegex().Match(paginationInfo);

        int currentPage = Int32.Parse(match.Groups["currentPage"].Value);
        int totalPages = Int32.Parse(match.Groups["totalPages"].Value);
        int totalRecords = Int32.Parse(match.Groups["totalRecords"].Value);

        var response = new QueryResponse<Cccf>(records, currentPage, totalPages, totalRecords);

        return response;
    }

    #region Regex

    [GeneratedRegex(@"\d{4}[-./]\d{2}[-./]\d{2}")]
    private static partial Regex DateRegex();

    [GeneratedRegex(@"共(?<totalRecords>\d+)条记录第(?<currentPage>\d+)/(?<totalPages>\d+)页")]
    private static partial Regex PaginationInfoRegex();

    #endregion Regex
}
