using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Firefly.Extensions;
using Firefly.Helpers;
using Firefly.Models.Requests;

namespace Firefly.Models;

public static partial class CccfFieldClassifier
{
    public static readonly string[] CertificateNumberKeywords = ["R0M", "R0S"];

    public static readonly string[] CertificateNumberPrefixes = ["Z", "JD", "S"];

    public static readonly string[] EnterpriseNameSuffixes = ["公司", "集团", "厂", "院", "设备", "制造", "科技", "股份", "有限", "责任", "会社", "TECH", "CO.", "Ltd", "LIMITED", "Inc", "Corp", "LLC", "Group"];

    public static readonly string[] ReportNumberPrefixes = ["Gn", "Dz", "Zb", "Xf", "FG", "XG", "No", "GN", "FD", "GJCXF", "FZ", "FC"];

    private static readonly IEnumerable<string> _cccfTestingCenterFields = EnumHelper.GetDescriptions<CccfTestingCenter>().Select(x => x.CleanText());

    public static CccfFieldType Predict(string field)
    {
        return Predict(new CccfFieldFeature(field));
    }

    public static CccfFieldType Predict(CccfFieldFeature fieldFeature)
    {
        if (String.IsNullOrWhiteSpace(fieldFeature.Field))
        {
            return CccfFieldType.SmartMode;
        }

        if (DateOnly.TryParse(fieldFeature.Field, out var date))
        {
            var now = DateOnly.FromDateTime(DateTime.Now);

            if (date < now)
            {
                return CccfFieldType.CertDateStart;
            }

            return CccfFieldType.CertDateEnd;
        }

        if (EnumHelper.GetDescriptions<CccfCertificateStatus>().Contains(fieldFeature.Field))
        {
            return CccfFieldType.Status;
        }

        if (GlobalData.MainTypeRegex().IsMatch(fieldFeature.Field))
        {
            return CccfFieldType.Model;
        }

        if (fieldFeature.OtherCharRatio > 0.5 && _cccfTestingCenterFields.Contains(fieldFeature.Field))
        {
            return CccfFieldType.TestingCenter;
        }

        if (fieldFeature.Field.ContainsAny(EnterpriseNameSuffixes)
            || (fieldFeature.Length >= 2
                && (GeoResources.ChinaLocations.Contains(fieldFeature.Field[..2])
                    || GeoResources.CountriesAndRegions.Contains(fieldFeature.Field[..2]))))
        {
            return CccfFieldType.EnterpriseName;
        }

        if (fieldFeature.OtherCharRatio > 0.4)
        {
            if (SubstringMatcher.IsSubstringPresent(CccfCatalogyResources.CccfCatalogs, fieldFeature.Field))
            {
                return CccfFieldType.ProductName;
            }

            return CccfFieldType.EnterpriseName;
        }

        string firstWord = FirstWordRegex().Match(fieldFeature.Field).Value;

        if (ReportNumberPrefixes.Contains(firstWord, StringComparer.OrdinalIgnoreCase))
        {
            return CccfFieldType.ReportNo;
        }

        if (CertificateNumberPrefixes.Contains(firstWord, StringComparer.OrdinalIgnoreCase)
            || fieldFeature.Field.ContainsAny(CertificateNumberKeywords))
        {
            return CccfFieldType.CertificateNo;
        }

        if (fieldFeature.NumberRatio > 0.8)
        {
            if (fieldFeature.Length > 14)
            {
                return CccfFieldType.CertificateNo;
            }

            return CccfFieldType.ReportNo;
        }

        if (fieldFeature.LetterRatio is > 0.4 and < 0.6
            || fieldFeature.NumberRatio is > 0.25 and < 0.4)
        {
            return CccfFieldType.Model;
        }

        return CccfFieldType.Model;
    }

    #region Regex

    [GeneratedRegex(@"^[a-zA-Z]+")]
    private static partial Regex FirstWordRegex();

    #endregion Regex
}
