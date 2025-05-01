using System.Linq;

using Firefly.Models.Responses;

namespace Firefly.Models;

/// <summary>
/// 表示可在页面上查找的 CCCF 对象。
/// </summary>
public class FindableCccf : IPageFindableObject
{
    public FindableText CertificateExpirationDate { get; }

    public FindableText CertificateIssuedDate { get; }

    public FindableText CertificateNumber { get; }

    public FindableText EnterpriseName { get; }

    public FindableText[] Models { get; }

    public FindableText ProductCategory { get; }

    public FindableText ProductName { get; }

    public FindableText[] ReportNumbers { get; }

    public FindableText Status { get; }

    public FindableCccf(Cccf cccf)
    {
        ProductCategory = cccf.ProductCategory;
        ProductName = cccf.ProductName;
        Models = [.. cccf.Models.Select(x => new FindableText(x))];
        EnterpriseName = cccf.EnterpriseName;
        CertificateNumber = cccf.CertificateNumber;
        ReportNumbers = [.. cccf.ReportNumbers.Select(x => new FindableText(x))];
        CertificateIssuedDate = cccf.CertificateIssuedDate.ToString("yyyy-MM-dd");
        CertificateExpirationDate = cccf.CertificateExpirationDate.ToString("yyyy-MM-dd");
        Status = cccf.Status;
    }
}
