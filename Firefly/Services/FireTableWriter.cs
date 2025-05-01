using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Firefly.Extensions;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Properties;
using Firefly.Services.Abstractions;

using NPOI.XWPF.UserModel;

namespace Firefly.Services;

public class FireTableWriter : IFireTableWriter
{
    public async Task WriteAsync(string filePath, XWPFDocument document, IEnumerable<FireCheckContext> fireCheckContexts)
    {
        foreach (var fireCheckContext in fireCheckContexts)
        {
            ClearBackgroundColor(fireCheckContext.FireProduct.Model);
            ClearBackgroundColor(fireCheckContext.FireProduct.EnterpriseName);
            ClearBackgroundColor(fireCheckContext.FireProduct.CertificateNumber);
            ClearBackgroundColor(fireCheckContext.FireProduct.ReportNumber);
            ClearBackgroundColor(fireCheckContext.FireProduct.ManufactureDate);

            ClearFireErrorDescriptions(fireCheckContext.FireProduct.Model);
            ClearFireErrorDescriptions(fireCheckContext.FireProduct.EnterpriseName);
            ClearFireErrorDescriptions(fireCheckContext.FireProduct.CertificateNumber);
            ClearFireErrorDescriptions(fireCheckContext.FireProduct.ReportNumber);
            ClearFireErrorDescriptions(fireCheckContext.FireProduct.ManufactureDate);

            if (fireCheckContext.HasError is not true)
            {
                continue;
            }

            bool modelError = CheckAndAppendErrors(fireCheckContext.FireProduct.Model, fireCheckContext.FireErrors,
                [
                FireErrors.MissingModel,
                FireErrors.MismatchedModel,
                FireErrors.InvalidModel]);

            bool entError = CheckAndAppendErrors(fireCheckContext.FireProduct.EnterpriseName, fireCheckContext.FireErrors,
                [
                FireErrors.MissingEnterpriseName,
                FireErrors.MismatchedEnterpriseName]);

            bool certNoError = CheckAndAppendErrors(fireCheckContext.FireProduct.CertificateNumber, fireCheckContext.FireErrors,
                [
                FireErrors.MissingCertificateNumber,
                FireErrors.MismatchedCertificateNumber,
                FireErrors.InvalidCertificateNumber,
                FireErrors.Irrelevant]);

            bool reportNoError = CheckAndAppendErrors(fireCheckContext.FireProduct.ReportNumber, fireCheckContext.FireErrors,
                [
                FireErrors.MissingReportNumber,
                FireErrors.MismatchedReportNumber,
                FireErrors.InvalidReportNumber]);

            bool dateError = CheckAndAppendErrors(fireCheckContext.FireProduct.ManufactureDate, fireCheckContext.FireErrors,
                [
                FireErrors.MissingManufactureDate,
                FireErrors.InvalidManufactureDate]);
        }

        await DocxHelper.WriteAsync(document, filePath);
    }

    private static bool AppendErrorDescription(FireCell fireCell, FireErrors fireErrors, FireErrors fireError, string fg, string bg)
    {
        if (!fireErrors.HasFlag(fireError))
        {
            return false;
        }

        fireCell.Cell.SetColor(bg);

        DocxHelper.AppendCellTextLine(fireCell.Cell, fireError.GetDescription(),
            Settings.Default.UseCustomFont ? Settings.Default.FontFamily : null,
            Settings.Default.UseCustomFont ? Settings.Default.FontSize : -1,
            fg: fg,
            bg: bg);

        return true;
    }

    private static bool CheckAndAppendErrors(FireCell? fireCell, FireErrors fireErrors, FireErrors[] errorTypes, string fg = "ff0000", string bg = "ffff00")
    {
        if (fireCell is null)
        {
            return false;
        }

        bool hasError = false;

        foreach (var error in errorTypes)
        {
            hasError |= AppendErrorDescription(fireCell, fireErrors, error, fg, bg);
        }

        return hasError;
    }

    private static void ClearBackgroundColor(FireCell? fireCell)
    {
        if (fireCell is null)
        {
            return;
        }

        DocxHelper.FillColor(fireCell.Cell, "auto");
    }

    private static void ClearFireErrorDescriptions(FireCell? fireCell)
    {
        if (fireCell is null)
        {
            return;
        }

        DocxHelper.RemoveAllParagraphs(fireCell.Cell, p => EnumHelper.GetDescriptions<FireErrors>().Contains(p));
    }
}
