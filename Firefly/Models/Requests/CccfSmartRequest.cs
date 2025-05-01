using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;

using Firefly.Models;
using Firefly.Models.Abstractions;
using Firefly.Models.Requests;

namespace Firefly.Services.Requests;

public partial class CccfSmartRequest : ObservableObject, ICccfRequest, IDeepCloneable<CccfSmartRequest>, IClearable, IEditableObject, IRevertibleChangeTracking
{
    #region Fields

    private CccfSmartRequest? _backup;

    private bool _inTxn = false;

    #endregion Fields

    #region Properties

    [ObservableProperty]
    public partial string Keyword { get; set; }

    [ObservableProperty]
    public partial CccfFieldType KeywordType { get; set; }

    [ObservableProperty]
    public partial int Page { get; set; }

    #endregion Properties

    #region Constructors

    public CccfSmartRequest(int page = 1, string keyword = "")
    {
        Page = page;
        Keyword = keyword;
        KeywordType = CccfFieldClassifier.Predict(keyword);
    }

    public CccfSmartRequest(int page, string keyword, CccfFieldType cccfFieldType)
    {
        Page = page;
        Keyword = keyword;
        KeywordType = cccfFieldType;
    }

    #endregion Constructors

    #region Methods

    public static implicit operator CccfRequest(CccfSmartRequest request)
    {
        return AsCccfRequest(request);
    }

    public CccfRequest AsCccfRequest()
    {
        return AsCccfRequest(this);
    }

    public void Predict()
    {
        KeywordType = CccfFieldClassifier.Predict(Keyword);
    }

    #endregion Methods

    #region ICccfRequest

    public static CccfRequest AsCccfRequest(CccfSmartRequest request)
    {
        var instance = new CccfRequest(request.Page);

        if (request.KeywordType == CccfFieldType.SmartMode)
        {
            return instance;
        }

        string propertyName = request.KeywordType.ToString();
        var propertyInfo = instance.GetType().GetProperty(propertyName);

        if (propertyInfo is not null)
        {
            object? value = request.Keyword;

            if (request.KeywordType is CccfFieldType.CertDateStart or CccfFieldType.CertDateEnd
                or CccfFieldType.IssuedDateStart or CccfFieldType.IssuedDateEnd)
            {
                if (DateOnly.TryParse(request.Keyword, out var date))
                {
                    value = date;
                }
                else
                {
                    value = null;
                }
            }

            propertyInfo.SetValue(instance, value);

            return instance;
        }

        throw new ArgumentException("未找到请求字段。", propertyName);
    }

    #endregion ICccfRequest

    #region IDeepCloneable

    public CccfSmartRequest DeepClone()
    {
        return new CccfSmartRequest(Page, Keyword, KeywordType);
    }

    #endregion IDeepCloneable

    #region IClearable

    public void Clear()
    {
        Page = 1;
        Keyword = "";
        KeywordType = CccfFieldType.SmartMode;
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
            Keyword = _backup.Keyword;
            KeywordType = _backup.KeywordType;

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

    [JsonIgnore]
    public bool IsChanged => _inTxn
        && _backup is not null
        && (Page != _backup.Page || Keyword != _backup.Keyword || KeywordType != _backup.KeywordType);

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
