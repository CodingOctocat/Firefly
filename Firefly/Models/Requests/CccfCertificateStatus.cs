using System.ComponentModel;
using System.Runtime.Serialization;

namespace Firefly.Models.Requests;

/// <summary>
/// CCCF 证书状态。
/// </summary>
public enum CccfCertificateStatus
{
    [Description("有效")]
    [EnumMember(Value = "1")]
    Active = 1,

    [Description("暂停")]
    [EnumMember(Value = "2")]
    Suspended = 2,

    [Description("注销")]
    [EnumMember(Value = "3")]
    Cancelled = 3,

    [Description("撤销")]
    [EnumMember(Value = "4")]
    Revoked = 4
}
