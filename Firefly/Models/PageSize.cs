using System.ComponentModel;

namespace Firefly.Models;

public enum PageSize
{
    //[Description("默认")]
    //Default = 0,

    [Description("5")]
    Five = 5,

    [Description("10")]
    Ten = 10,

    [Description("15")]
    Fifteen = 15,

    [Description("20")]
    Twenty = 20,

    [Description("25")]
    TwentyFive = 25,

    [Description("50")]
    Fifty = 50,

    [Description("100")]
    OneHundred = 100,

    [Description("150")]
    OneFifty = 150,

    [Description("200")]
    TwoHundred = 200,

    [Description("500")]
    FiveHundred = 500,

    [Description("1000")]
    OneThousand = 1000,

    //[Description("无限制")]
    //Unlimited = -1
}
