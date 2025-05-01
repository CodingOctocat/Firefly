using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Firefly.Models;

public static partial class GlobalData
{
    public static readonly JsonSerializerOptions PrettyJsonOptions = new() {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IgnoreReadOnlyProperties = true
    };

    [GeneratedRegex(@"\(?(主|分)[\u4E00-\u9fA5]?型\)?")]
    public static partial Regex MainTypeRegex();
}
