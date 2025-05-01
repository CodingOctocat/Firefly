using System;
using System.Linq;

using Firefly.Extensions;
using Firefly.Models;
using Firefly.Models.Responses;
using Firefly.Services.Abstractions;

namespace Firefly.Services;

public class CccfChecker : IFireChecker<Cccf>
{
    private static readonly DateOnly _dt2003 = new(2003, 1, 1);

    private readonly IFireCheckSettings _fireCheckSettings;

    private FireErrors _fireErrors;

    public CccfChecker(IFireCheckSettings fireCheckSettings)
    {
        _fireCheckSettings = fireCheckSettings;
    }

    public FireErrors Check(FireProduct fireProduct, Cccf? target)
    {
        CheckModel(fireProduct, target);
        CheckEnterpriseName(fireProduct, target);
        CheckCertificateNumber(fireProduct, target);
        CheckReportNumber(fireProduct, target);
        CheckManufactureDate(fireProduct, target);

        if (target is not null)
        {
            double ratio = GetProductNameRatio(fireProduct, target);

            if (ratio == 0)
            {
                _fireErrors |= FireErrors.Irrelevant;
            }
        }

        if (_fireErrors == FireErrors.Unknown)
        {
            _fireErrors = FireErrors.None;
        }

        return _fireErrors;
    }

    /// <summary>
    /// 对消防产品进行简单的预检查。
    /// </summary>
    /// <param name="fireProduct"></param>
    /// <returns>如果预检查通过，返回 <see langword="true"/>，表示无需进行后续 <see cref="Check(FireProduct, Cccf?)"/> 流程，否则，返回 <see langword="false"/>。</returns>
    public bool PreCheck(FireProduct fireProduct, out FireErrors fireErrors)
    {
        _fireErrors = FireErrors.Unknown;

        if (fireProduct.IsRuleSkipped)
        {
            _fireErrors = FireErrors.RuleSkipped;
            fireErrors = _fireErrors;

            return true;
        }

        string certReportNumberText = $"{fireProduct.CertificateNumber.Text}/{fireProduct.ReportNumber?.Text}".TrimSlash();
        string modelEnterpriseText = $"{fireProduct.Model.Text}/{fireProduct.EnterpriseName.Text}".TrimSlash();

        if ((certReportNumberText == "/" && modelEnterpriseText == "/") || certReportNumberText == modelEnterpriseText)
        {
            _fireErrors = FireErrors.ContentSkipped;
            fireErrors = _fireErrors;

            return true;
        }

        fireErrors = _fireErrors;

        return false;
    }

    private static double GetProductNameRatio(FireProduct fireProduct, Cccf target)
    {
        int green = 0;
        int red = 0;

        var names = fireProduct.Name.Text.Distinct();

        foreach (char n in target.ProductName)
        {
            if (names.Contains(n))
            {
                green++;
            }
            else
            {
                red++;
            }
        }

        double ratio = (double)green / (green + red);

        return ratio;
    }

    private void CheckCertificateNumber(FireProduct fireProduct, Cccf? target)
    {
        if (fireProduct.CertificateNumber.Text == "/")
        {
            return;
        }

        if (String.IsNullOrWhiteSpace(fireProduct.CertificateNumber.Text))
        {
            _fireErrors |= FireErrors.MissingCertificateNumber;
        }
        else
        {
            if (target is null)
            {
                _fireErrors |= FireErrors.InvalidCertificateNumber;
            }
            else
            {
                if (!CompareByStrictMode(fireProduct.CertificateNumber, target.CertificateNumber))
                {
                    _fireErrors |= FireErrors.MismatchedCertificateNumber;
                }
            }
        }
    }

    private void CheckEnterpriseName(FireProduct fireProduct, Cccf? target)
    {
        if (fireProduct.EnterpriseName.Text == "/")
        {
            return;
        }

        if (String.IsNullOrWhiteSpace(fireProduct.EnterpriseName.Text))
        {
            _fireErrors |= FireErrors.MissingEnterpriseName;
        }
        else if (target is not null && !CompareByStrictMode(fireProduct.EnterpriseName, target.EnterpriseName))
        {
            _fireErrors |= FireErrors.MismatchedEnterpriseName;
        }
    }

    private void CheckManufactureDate(FireProduct fireProduct, Cccf? target)
    {
        if (fireProduct.ManufactureDate?.Text is null or "/")
        {
            return;
        }

        if (String.IsNullOrWhiteSpace(fireProduct.ManufactureDate.Text))
        {
            _fireErrors |= FireErrors.MissingManufactureDate;
        }
        else if (DateOnly.TryParse(fireProduct.ManufactureDate.Text, out var date))
        {
            if (_fireCheckSettings.CheckManufactureDate)
            {
                if (date > DateOnly.FromDateTime(DateTime.Now) || date < _dt2003)
                {
                    _fireErrors |= FireErrors.InvalidManufactureDate;
                }
                else if (target is not null && (date > target.CertificateExpirationDate || date < target.CertificateIssuedDate))
                {
                    _fireErrors |= FireErrors.InvalidManufactureDate;
                }
            }
        }
        else
        {
            _fireErrors |= FireErrors.InvalidManufactureDate;
        }
    }

    private void CheckModel(FireProduct fireProduct, Cccf? target)
    {
        if (fireProduct.Model.Text == "/")
        {
            return;
        }

        if (String.IsNullOrWhiteSpace(fireProduct.Model.Text))
        {
            _fireErrors |= FireErrors.MissingModel;
        }
        else
        {
            if (target is null)
            {
                _fireErrors |= FireErrors.InvalidModel;
            }
            else
            {
                string model = GlobalData.MainTypeRegex().Replace(GetTextByStrictMode(fireProduct.Model), "");
                bool match = target.Models.Any(m => model == GlobalData.MainTypeRegex().Replace(GetTextByStrictMode(m), ""));

                if (!match)
                {
                    _fireErrors |= FireErrors.MismatchedModel;
                }
            }
        }
    }

    private void CheckReportNumber(FireProduct fireProduct, Cccf? target)
    {
        if (fireProduct.ReportNumber?.Text is null or "/")
        {
            return;
        }

        if (String.IsNullOrWhiteSpace(fireProduct.ReportNumber.Text))
        {
            if (_fireCheckSettings.CheckReportNumber)
            {
                _fireErrors |= FireErrors.MissingReportNumber;
            }
        }
        else
        {
            if (target is null)
            {
                _fireErrors |= FireErrors.InvalidReportNumber;
            }
            else
            {
                string reportNo = GetTextByStrictMode(fireProduct.ReportNumber);
                bool match = target.ReportNumbers.Any(r => reportNo == GetTextByStrictMode(r));

                if (!match)
                {
                    _fireErrors |= FireErrors.MismatchedReportNumber;
                }
            }
        }
    }

    private bool CompareByStrictMode(FireCell fireCell, string cccf)
    {
        string text = GetTextByStrictMode(fireCell);

        if (_fireCheckSettings.StrictMode)
        {
            return text == cccf;
        }

        return text == cccf.CleanText();
    }

    private string GetTextByStrictMode(FireCell fireCell)
    {
        if (_fireCheckSettings.StrictMode)
        {
            return fireCell.Raw;
        }

        return fireCell.Text;
    }

    private string GetTextByStrictMode(string cccf)
    {
        if (_fireCheckSettings.StrictMode)
        {
            return cccf;
        }

        return cccf.CleanText();
    }
}
