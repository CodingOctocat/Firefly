using System;
using System.ComponentModel;

namespace Firefly.Models;

[Flags]
public enum FireErrors
{
    [Description("<未知>")]
    Unknown = 0,

    [Description("<规则跳过>")]
    RuleSkipped = 1 << 0,

    [Description("<内容跳过>")]
    ContentSkipped = 1 << 1,

    [Description("<正常>")]
    None = 1 << 2,

    [Description("缺少产品型号")]
    MissingModel = 1 << 3,

    [Description("缺少生产厂家")]
    MissingEnterpriseName = 1 << 4,

    [Description("缺少检验报告")]
    MissingReportNumber = 1 << 5,

    [Description("缺少证书编号")]
    MissingCertificateNumber = 1 << 6,

    [Description("缺少出厂日期")]
    MissingManufactureDate = 1 << 7,

    [Description("产品型号不匹配")]
    MismatchedModel = 1 << 8,

    [Description("生产厂家不匹配")]
    MismatchedEnterpriseName = 1 << 9,

    [Description("检验报告不匹配")]
    MismatchedReportNumber = 1 << 10,

    [Description("证书编号不匹配")]
    MismatchedCertificateNumber = 1 << 11,

    [Description("未查询到型号")]
    InvalidModel = 1 << 12,

    [Description("未查询到生产厂家")]
    InvalidEnterprise = 1 << 13,

    [Description("未查询到检验报告")]
    InvalidReportNumber = 1 << 14,

    [Description("未查询到证书编号")]
    InvalidCertificateNumber = 1 << 15,

    [Description("出厂日期无效")]
    InvalidManufactureDate = 1 << 16,

    [Description("<证书与产品可能不相关>")]
    Irrelevant = 1 << 17
}
