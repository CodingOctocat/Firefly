using System.ComponentModel;
using System.Runtime.Serialization;

namespace Firefly.Models.Requests;

/// <summary>
/// CCCF 检验中心。
/// </summary>
public enum CccfTestingCenter
{
    [Description("国家消防产品质量监督检验中心（广东）")]
    [EnumMember(Value = "国家消防产品质量监督检验中心（广东）")]
    TestingCenter1,

    [Description("国家消防及阻燃产品质量监督检验中心（山东）")]
    [EnumMember(Value = "国家消防及阻燃产品质量监督检验中心（山东）")]
    TestingCenter2,

    [Description("国家消防产品质量监督检验中心（江苏）")]
    [EnumMember(Value = "国家消防产品质量监督检验中心（江苏）")]
    TestingCenter3,

    [Description("国家五金工具及门类产品质量监督检验中心(浙江)")]
    [EnumMember(Value = "国家五金工具及门类产品质量监督检验中心(浙江)")]
    TestingCenter4,

    [Description("国家消防装备质量监督检验中心")]
    [EnumMember(Value = "国家消防装备质量监督检验中心")]
    TestingCenter5,

    [Description("国家消防电子产品质量监督检验中心")]
    [EnumMember(Value = "国家消防电子产品质量监督检验中心")]
    TestingCenter6,

    [Description("国家防火建筑材料质量监督检验中心")]
    [EnumMember(Value = "国家防火建筑材料质量监督检验中心")]
    TestingCenter7,

    [Description("国家固定灭火系统和耐火构件质量监督检验中心")]
    [EnumMember(Value = "国家固定灭火系统和耐火构件质量监督检验中心")]
    TestingCenter8,
}
