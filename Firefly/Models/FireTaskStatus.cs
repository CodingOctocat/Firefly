using System.ComponentModel;

namespace Firefly.Models;

public enum FireTaskStatus
{
    None,

    [Description("正常")]
    Normal,

    [Description("已完成")]
    Completed,

    [Description("已取消")]
    Cancelled,

    [Description("错误")]
    Error
}
