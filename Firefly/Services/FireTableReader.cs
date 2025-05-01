using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using Firefly.Extensions;
using Firefly.Helpers;
using Firefly.Services.Abstractions;

using NPOI.XWPF.UserModel;

namespace Firefly.Models;

public sealed partial class FireTableReader : IFireTableReader
{
    public FireTableReader()
    {
        FireCheckContext.ResetOrder();
        FireSystem.ResetOrder();
    }

    public IEnumerable<FireProduct> Read(XWPFDocument doc, FireTableColumnMapping columnMapping)
    {
        var tables = DocxHelper.GetTables(doc);
        string fireSystem = "<未知系统>";

        foreach (var table in tables)
        {
            foreach (var row in table.Rows)
            {
                int colCount = row.GetTableCells().Count;

                if (columnMapping.IsFireSystemFullRow is true && colCount == 1)
                {
                    string title = DocxHelper.GetCellCleanText(row.GetCell(0), h: true, v: true);
                    fireSystem = String.IsNullOrWhiteSpace(title) ? "<未知系统>" : title;

                    continue;
                }

                if (colCount != columnMapping.RuleColumns)
                {
                    continue;
                }

                if (columnMapping.IsFireSystemFullRow is false)
                {
                    var cell = row.GetCell(columnMapping.FireSystemColumn - 1);
                    string? title;

                    if (DocxHelper.IsVMergeCell(cell))
                    {
                        title = DocxHelper.GetVMergeCellText(row, columnMapping.FireSystemColumn - 1);
                    }
                    else
                    {
                        title = DocxHelper.GetCellCleanText(cell);
                    }

                    fireSystem = String.IsNullOrWhiteSpace(title) ? "<未知系统>" : title;
                }

                if (!TryGetProductName(row, columnMapping, out var name))
                {
                    continue;
                }

                var c1 = CreateFireCell(row, columnMapping.C1Column);
                var count = CreateFireCell(row, columnMapping.CountColumn);

                var (model, ent, modelEnterprise) = CreateModelAndEnterpriseNameFireCell(row, columnMapping);
                var (certNo, reportNo, certs) = CreateCertificateAndReportNumberFireCell(row, columnMapping);

                var status = CreateFireCell(row, columnMapping.StatusColumn);
                var mfgDate = CreateFireCell(row, columnMapping.ManufactureDateColumn);
                var remark = CreateFireCell(row, columnMapping.RemarkColumn);

                bool skip = IsSkipRow(row, columnMapping);

                var product = new FireProduct(
                    fireSystem,
                    c1,
                    name,
                    count,
                    modelEnterprise,
                    model,
                    ent,
                    certs,
                    certNo,
                    reportNo,
                    status,
                    mfgDate,
                    remark,
                    skip);

                yield return product;
            }
        }
    }

    private static (FireCell CertificateNumber, FireCell? ReportNumber, FireCell? CertificateAndReportNumber) CreateCertificateAndReportNumberFireCell(XWPFTableRow row, FireTableColumnMapping columnMapping)
    {
        XWPFTableCell? certsCell;
        FireCell? certs = null;
        FireCell certNo;
        FireCell? reportNo;

        if (columnMapping.IsCertificateAndReportNumberInSameColumn)
        {
            certsCell = row.GetCell(columnMapping.CertificateNumberColumn - 1);
            certs = new FireCell(certsCell);

            (string certStr, string reportStr) = ParseCertificateAndReportNumber(certs.Text);
            certNo = new FireCell(certsCell, certStr);
            reportNo = new FireCell(certsCell, reportStr);
        }
        else
        {
            certNo = CreateFireCell(row, columnMapping.CertificateNumberColumn)!;
            reportNo = CreateFireCell(row, columnMapping.ReportNumberColumn);
        }

        return (certNo, reportNo, certs);
    }

    private static FireCell? CreateFireCell(XWPFTableRow row, int col)
    {
        if (col <= 0)
        {
            return null;
        }

        return new FireCell(row.GetCell(col - 1));
    }

    private static (FireCell Model, FireCell EnterpriseName, FireCell? ModelAndEnterpriseName) CreateModelAndEnterpriseNameFireCell(XWPFTableRow row, FireTableColumnMapping columnMapping)
    {
        XWPFTableCell? modelEnterpriseCell;
        FireCell? modelEnterprise = null;
        FireCell model, ent;

        if (columnMapping.IsModelAndEnterpriseNameInSameColumn)
        {
            modelEnterpriseCell = row.GetCell(columnMapping.ModelColumn - 1);
            modelEnterprise = new FireCell(modelEnterpriseCell);
            (string modelStr, string entStr) = ParseModelAndEnterpriseName(modelEnterprise.Text);
            model = new FireCell(modelEnterpriseCell, modelStr);
            ent = new FireCell(modelEnterpriseCell, entStr);
        }
        else
        {
            model = CreateFireCell(row, columnMapping.ModelColumn)!;
            ent = CreateFireCell(row, columnMapping.EnterpriseNameColumn)!;
        }

        return (model, ent, modelEnterprise);
    }

    private static bool IsSkipRow(XWPFTableRow row, FireTableColumnMapping columnMapping)
    {
        foreach (var rule in columnMapping.SkipRowRules)
        {
            string targetText = DocxHelper.GetCellText(row.GetCell(rule.TargetColumn - 1));

            if (rule.IsSkip(targetText))
            {
                return true;
            }
        }

        return false;
    }

    private static (string Certificate, string Report) ParseCertificateAndReportNumber(string text)
    {
        text = text.RemoveWhiteSpaces(v: false);

        if (text == "/")
        {
            return ("/", "/");
        }

        if (String.IsNullOrWhiteSpace(text))
        {
            return ("", "");
        }

        string[] lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length == 0)
        {
            return ("", "");
        }

        if (lines.Length == 1)
        {
            string line = lines[0];

            string[] fragments = line.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (fragments.Length == 1)
            {
                if (CccfFieldClassifier.Predict(line) == CccfFieldType.CertificateNo)
                {
                    return (line, "");
                }

                if (CccfFieldClassifier.Predict(line) == CccfFieldType.ReportNo)
                {
                    return ("", line);
                }

                if (line.Length > 14)
                {
                    return (line, "");
                }

                return ("", line);
            }

            if (fragments.Length == 2)
            {
                return ParseCertificateAndReportNumber(fragments[0], fragments[1]);
            }
        }

        if (lines.Length == 2)
        {
            return ParseCertificateAndReportNumber(lines[0], lines[1]);
        }

        return ("", "");
    }

    private static (string Certificate, string Report) ParseCertificateAndReportNumber(string a, string b)
    {
        if (CccfFieldClassifier.Predict(b) == CccfFieldType.CertificateNo)
        {
            return (b, a);
        }

        if (CccfFieldClassifier.Predict(a) == CccfFieldType.ReportNo)
        {
            return (b, a);
        }

        if (CccfFieldClassifier.Predict(a) == CccfFieldType.CertificateNo)
        {
            return (a, b);
        }

        if (CccfFieldClassifier.Predict(b) == CccfFieldType.ReportNo)
        {
            return (a, b);
        }

        if (b.Length >= a.Length)
        {
            return (b, a);
        }

        return (a, b);
    }

    private static (string Model, string EnterpriseName) ParseModelAndEnterpriseName(string text)
    {
        if (text == "/")
        {
            return ("/", "/");
        }

        if (String.IsNullOrWhiteSpace(text))
        {
            return ("", "");
        }

        string[] lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length == 1)
        {
            string line = lines[0].Trim(' ', '/');

            if (line.Length < 2)
            {
                int c = ChineseCharRegex().Count(line);

                return c == 2 ? ("", line) : (line, "");
            }

            string twoChars = line[..2];
            int entStartIndex = -1;

            // 查找生产厂家的开始索引
            for (int i = 0; i < line.Length - 1; i++)
            {
                twoChars = line.Substring(i, 2);

                if (GeoResources.ChinaLocations.Contains(twoChars) || GeoResources.CountriesAndRegions.Contains(twoChars))
                {
                    entStartIndex = i;

                    break;
                }
            }

            // 匹配类似 xxx(地区)xxx 的情况，重新设置生产厂家的开始索引
            if (line.Contains($"({twoChars})"))
            {
                var match = Regex.Match(line, $@"[\u4E00-\u9fA5]+\({Regex.Escape(twoChars)}\)");

                if (match.Success)
                {
                    entStartIndex = match.Index;
                }
            }

            // 查找生产厂家的结束索引
            int entEndIndex = line.EndIndexOfAny(["公司", "厂", "院"]);

            if (entStartIndex == 0)
            {
                if (entEndIndex != -1)
                {
                    string ent = line[..(entEndIndex + 1)];
                    string model = line[(entEndIndex + 1)..];

                    return (model.Trim(' ', '/'), ent.Trim(' ', '/'));
                }

                var match = EnterpriseModelRegex().Match(line);

                if (match.Success)
                {
                    return (match.Groups[2].Value.Trim(' ', '/'), match.Groups[1].Value.Trim(' ', '/'));
                }

                return ("", line);
            }

            if (entEndIndex == line.Length - 1)
            {
                if (entStartIndex != -1)
                {
                    string ent = line[entStartIndex..];
                    string model = line[..entStartIndex];

                    return (model.Trim(' ', '/'), ent.Trim(' ', '/'));
                }

                var match = ModelAndEnterpriseRegex().Match(line);

                if (match.Success)
                {
                    return (match.Groups[1].Value.Trim(' ', '/'), match.Groups[2].Value.Trim(' ', '/'));
                }

                return (line, "");
            }

            int cc = ChineseCharRegex().Count(line);

            return cc >= line.Length / 5 ? ("", line) : (line, "");
        }

        if (lines.Length == 2)
        {
            if (lines[1].Length >= 2)
            {
                string startStr = lines[1][..2];

                if (GeoResources.ChinaLocations.Contains(startStr, StringComparison.OrdinalIgnoreCase)
                    || GeoResources.CountriesAndRegions.Contains(startStr, StringComparison.OrdinalIgnoreCase)
                    || lines[1].ContainsAny(CccfFieldClassifier.EnterpriseNameSuffixes))
                {
                    return (lines[0], lines[1]);
                }
            }

            if (lines[0].Length >= 2)
            {
                string startStr = lines[0][..2];

                if (GeoResources.ChinaLocations.Contains(startStr, StringComparison.OrdinalIgnoreCase)
                    || GeoResources.CountriesAndRegions.Contains(startStr, StringComparison.OrdinalIgnoreCase)
                    || lines[0].ContainsAny(CccfFieldClassifier.EnterpriseNameSuffixes))
                {
                    return (lines[1], lines[0]);
                }
            }

            int lc1 = ChineseCharRegex().Count(lines[0]);
            int lc2 = ChineseCharRegex().Count(lines[1]);

            return lc2 >= lc1 ? (lines[0], lines[1]) : (lines[1], lines[0]);
        }

        return ("", "");
    }

    /// <summary>
    /// 尝试获取产品名称。
    /// </summary>
    /// <param name="row"></param>
    /// <param name="name"></param>
    /// <returns><see langword="true"/>：已命名的产品；<see langword="false"/>：未命名的产品。
    /// </returns>
    private static bool TryGetProductName(XWPFTableRow row, FireTableColumnMapping columnMapping, [NotNullWhen(true)] out FireCell? name)
    {
        var cell = row.GetCell(columnMapping.ProductNameColumn - 1);

        if (DocxHelper.IsVMergeContinueCell(cell))
        {
            name = new FireCell(cell, DocxHelper.GetVMergeCellText(row, columnMapping.ProductNameColumn - 1, false) ?? "");
        }
        else
        {
            name = new FireCell(cell, DocxHelper.GetCellText(cell));
        }

        return !String.IsNullOrWhiteSpace(name.Text);
    }

    #region Regex

    [GeneratedRegex(@"[\u4E00-\u9fA5]")]
    private static partial Regex ChineseCharRegex();

    [GeneratedRegex(@"([\u4E00-\u9fA5\(\)]+)(.*)")]
    private static partial Regex EnterpriseModelRegex();

    [GeneratedRegex(@"([^\u4E00-\u9fA5]*)([\u4E00-\u9fA5\(\)]+)")]
    private static partial Regex ModelAndEnterpriseRegex();

    #endregion Regex
}
