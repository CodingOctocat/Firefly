using System.ComponentModel;

namespace Firefly.Models;

public enum CccfFieldType
{
    [Description("自动识别")]
    SmartMode,

    [Description("企业名称")]
    EnterpriseName,

    [Description("产品名称")]
    ProductName,

    [Description("产品型号")]
    Model,

    [Description("证书编号")]
    CertificateNo,

    [Description("检验报告")]
    ReportNo,

    [Description("证书状态")]
    Status,

    [Description("发(换)证日期")]
    CertDateStart,

    [Description("发(换)证日期(截止)")]
    CertDateEnd,

    [Description("报告签发日期")]
    IssuedDateStart,

    [Description("报告签发日期(截止)")]
    IssuedDateEnd,

    [Description("检验中心")]
    TestingCenter,
}
