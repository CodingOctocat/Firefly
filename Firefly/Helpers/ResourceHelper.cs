using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Firefly.Helpers;

public static class ResourceHelper
{
    public static async Task<string> ReadAllTextAsync(string relativeUri)
    {
        using var stream = Application.GetResourceStream(new Uri($"pack://application:,,,/{relativeUri}")).Stream;
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }
}
