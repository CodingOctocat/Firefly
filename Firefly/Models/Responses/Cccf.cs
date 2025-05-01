using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firefly.Models.Responses;

[Table("Products")]
[Index(nameof(ProductName), nameof(EnterpriseName), nameof(Models), nameof(ReportNumbers))]
public record Cccf : IPageFindable<FindableText>
{
    private static readonly string _certificateNumberHyperlink;
    private static readonly string _productNameHyperlink;

    [Column("cert_exp")]
    public DateOnly CertificateExpirationDate { get; private set; }

    [Column("cert_iss")]
    public DateOnly CertificateIssuedDate { get; private set; }

    [Key]
    [Column("cert_no")]
    public string CertificateNumber { get; private set; }

    public Uri CertificateNumberHyperlink { get; }

    [Column("ent")]
    public string EnterpriseName { get; private set; }

    /// <summary>
    /// 获取消防产品的主型（如果存在）或/且排序第一位的型号。
    /// </summary>
    public string? Model { get; }

    [Column("models")]
    public string[] Models { get; }

    [Column("cat")]
    public string ProductCategory
    {
        get => field ?? "";
        set => field = value ?? "";
    }

    [Column("name")]
    public string ProductName { get; private set; }

    public Uri ProductNameHyperlink { get; }

    /// <summary>
    /// 获取排序第一位的检验报告号。
    /// </summary>
    public string? ReportNumber { get; }

    [Column("rpt_nos")]
    public string[] ReportNumbers { get; }

    [Column("status")]
    public string Status { get; private set; }

    public FindableCccf FindableObject { get; }

    public Cccf(string productCategory, string productName, string[] models, string enterpriseName,
        string certificateNumber, string[] reportNumbers,
        DateOnly certificateIssuedDate, DateOnly certificateExpirationDate, string status)
    {
        ProductCategory = productCategory;
        ProductName = productName;
        ProductNameHyperlink = GetProductNameHyperlink(certificateNumber);
        Models = models;
        Model = models.FirstOrDefault(m => GlobalData.MainTypeRegex().IsMatch(m)) ?? models.FirstOrDefault();
        EnterpriseName = enterpriseName;
        CertificateNumber = certificateNumber;
        CertificateNumberHyperlink = GetCertificateNumberHyperlink(certificateNumber);
        ReportNumbers = reportNumbers;
        ReportNumber = reportNumbers.FirstOrDefault();
        CertificateIssuedDate = certificateIssuedDate;
        CertificateExpirationDate = certificateExpirationDate;
        Status = status;
        FindableObject = new FindableCccf(this);
    }

    static Cccf()
    {
        var configuration = App.Current.Services.GetRequiredService<IConfiguration>();
        string? baseUrl = configuration["CccfApi:BaseUrl"];
        _certificateNumberHyperlink = $"{baseUrl}{configuration["CccfApi:CertificateNumberHyperlink"]}";
        _productNameHyperlink = $"{baseUrl}{configuration["CccfApi:ProductNameHyperlink"]}";
    }

    public override string ToString()
    {
        return $"{ProductCategory}/{ProductName}: {Model}/{EnterpriseName} - {CertificateNumber} - {ReportNumber} | {CertificateIssuedDate:yyyy-MM-dd} - {CertificateExpirationDate:yyyy-MM-dd} - {Status}";
    }

    private static Uri GetCertificateNumberHyperlink(string certNo)
    {
        return new Uri(String.Format(_certificateNumberHyperlink, certNo));
    }

    private static Uri GetProductNameHyperlink(string certNo)
    {
        return new Uri(String.Format(_productNameHyperlink, certNo));
    }

    #region IPageFindable<FindableText>

    public IEnumerable<FindableText> GetFindableInfos()
    {
        yield return FindableObject.ProductCategory;
        yield return FindableObject.ProductName;

        foreach (var model in FindableObject.Models)
        {
            yield return model;
        }

        yield return FindableObject.EnterpriseName;
        yield return FindableObject.CertificateNumber;

        foreach (var reportNumber in FindableObject.ReportNumbers)
        {
            yield return reportNumber;
        }

        yield return FindableObject.CertificateIssuedDate;
        yield return FindableObject.CertificateExpirationDate;
        yield return FindableObject.Status;
    }

    #endregion IPageFindable<FindableText>
}
