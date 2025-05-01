using System;
using System.Collections.Generic;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Abstractions;
using Firefly.Models.Requests;

namespace Firefly.Services.Requests;

public partial class CccfRequest : ObservableObject, ICccfRequest, IDeepCloneable<CccfRequest>, IClearable, IEditableObject, IRevertibleChangeTracking
{
    /*
     * 此类的属性命名尽量与 CCCF 网站后端相同。
     */

    #region Fields

    private CccfRequest? _backup;

    private bool _inTxn = false;

    #endregion Fields

    #region Properties

    /// <summary>
    /// 发(换)证日期(截止)。
    /// </summary>
    [ObservableProperty]
    public partial DateOnly? CertDateEnd { get; set; }

    /// <summary>
    /// 发(换)证日期。
    /// </summary>
    [ObservableProperty]
    public partial DateOnly? CertDateStart { get; set; }

    [ObservableProperty]
    public partial string CertificateNo { get; set; } = "";

    [ObservableProperty]
    public partial string EnterpriseName { get; set; } = "";

    /// <summary>
    /// 报告签发日期(截止)。
    /// </summary>
    [ObservableProperty]
    public partial DateOnly? IssuedDateEnd { get; set; }

    /// <summary>
    /// 报告签发日期。
    /// </summary>
    [ObservableProperty]
    public partial DateOnly? IssuedDateStart { get; set; }

    [ObservableProperty]
    public partial string Model { get; set; } = "";

    [ObservableProperty]
    public partial int Page { get; set; } = 1;

    [ObservableProperty]
    public partial string ProductName { get; set; } = "";

    [ObservableProperty]
    public partial string ReportNo { get; set; } = "";

    /// <summary>
    /// 证书状态。
    /// </summary>
    [ObservableProperty]
    public partial string Status { get; set; } = "";

    /// <summary>
    /// 检验中心。
    /// </summary>
    [ObservableProperty]
    public partial string TestingCenter { get; set; } = "";

    #endregion Properties

    #region Constructors

    public CccfRequest(int page = 1,
        string productName = "",
        string model = "",
        string enterpriseName = "",
        string certificateNo = "",
        string reportNo = "")
    {
        Page = page;
        ProductName = productName;
        Model = model;
        EnterpriseName = enterpriseName;
        CertificateNo = certificateNo;
        ReportNo = reportNo;
    }

    #endregion Constructors

    #region Methods

    public IEnumerable<KeyValuePair<string, string>> ToNameValueCollection()
    {
        yield return new KeyValuePair<string, string>("page", Page.ToString());
        yield return new KeyValuePair<string, string>("enterpriseName", EnterpriseName);
        yield return new KeyValuePair<string, string>("productName", ProductName);
        yield return new KeyValuePair<string, string>("model", Model);
        yield return new KeyValuePair<string, string>("certificateNo", CertificateNo);
        yield return new KeyValuePair<string, string>("reportNo", ReportNo);
        yield return new KeyValuePair<string, string>("inState", ConvertStatus(Status));
        yield return new KeyValuePair<string, string>("certDateStart", ConvertDateOnly(CertDateStart));
        yield return new KeyValuePair<string, string>("certDateEnd", ConvertDateOnly(CertDateEnd));
        yield return new KeyValuePair<string, string>("issuedDateStart", ConvertDateOnly(IssuedDateStart));
        yield return new KeyValuePair<string, string>("issuedDateEnd", ConvertDateOnly(IssuedDateEnd));
        yield return new KeyValuePair<string, string>("testOrg", TestingCenter);
    }

    private static string ConvertDateOnly(DateOnly? date)
    {
        if (date is null)
        {
            return "";
        }

        return date.Value.ToString("yyyy-MM-dd");
    }

    private static string ConvertStatus(string value)
    {
        if (EnumHelper.GetDescriptionEnumMemberDict<CccfCertificateStatus>().TryGetValue(value, out string? status))
        {
            return status;
        }

        return value;
    }

    #endregion Methods

    #region ICccfRequest

    public CccfRequest AsCccfRequest()
    {
        return this;
    }

    #endregion ICccfRequest

    #region IDeepCloneable

    public CccfRequest DeepClone()
    {
        return new CccfRequest(Page, ProductName, Model, EnterpriseName, CertificateNo, ReportNo) {
            Status = Status,
            CertDateStart = CertDateStart,
            CertDateEnd = CertDateEnd,
            IssuedDateStart = IssuedDateStart,
            IssuedDateEnd = IssuedDateEnd,
            TestingCenter = TestingCenter
        };
    }

    #endregion IDeepCloneable

    #region IClearable

    public void Clear()
    {
        Page = 1;
        EnterpriseName = "";
        ProductName = "";
        Model = "";
        CertificateNo = "";
        ReportNo = "";
        Status = "";
        CertDateStart = null;
        CertDateEnd = null;
        IssuedDateStart = null;
        IssuedDateEnd = null;
        TestingCenter = "";
    }

    #endregion IClearable

    #region IEditableObject

    public void BeginEdit()
    {
        if (!_inTxn)
        {
            _backup = DeepClone();
            _inTxn = true;
            OnPropertyChanged(nameof(IsChanged));
        }
    }

    public void CancelEdit()
    {
        if (_inTxn)
        {
            Page = _backup!.Page;
            EnterpriseName = _backup.EnterpriseName;
            ProductName = _backup.ProductName;
            Model = _backup.Model;
            CertificateNo = _backup.CertificateNo;
            ReportNo = _backup.ReportNo;
            Status = _backup.Status;
            CertDateStart = _backup.CertDateStart;
            CertDateEnd = _backup.CertDateEnd;
            IssuedDateStart = _backup.IssuedDateStart;
            IssuedDateEnd = _backup.IssuedDateEnd;
            TestingCenter = _backup.TestingCenter;

            _inTxn = false;
            OnPropertyChanged(nameof(IsChanged));
        }
    }

    public void EndEdit()
    {
        if (_inTxn)
        {
            _backup = new();
            _inTxn = false;
            OnPropertyChanged(nameof(IsChanged));
        }
    }

    #endregion IEditableObject

    #region IRevertibleChangeTracking

    public bool IsChanged => _inTxn
        && _backup is not null
        && (Page != _backup.Page
            || EnterpriseName != _backup.EnterpriseName
            || ProductName != _backup.ProductName
            || Model != _backup.Model
            || CertificateNo != _backup.CertificateNo
            || ReportNo != _backup.CertificateNo
            || Status != _backup.Status
            || CertDateStart != _backup.CertDateStart
            || CertDateEnd != _backup.CertDateEnd
            || IssuedDateStart != _backup.IssuedDateStart
            || IssuedDateEnd != _backup.IssuedDateEnd
            || TestingCenter != _backup.TestingCenter);

    public void AcceptChanges()
    {
        EndEdit();
        BeginEdit();
    }

    public void RejectChanges()
    {
        CancelEdit();
        BeginEdit();
    }

    #endregion IRevertibleChangeTracking
}
