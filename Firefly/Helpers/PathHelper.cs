using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Firefly.Helpers;

public static class PathHelper
{
    [return: NotNullIfNotNull(nameof(path))]
    public static string? GetRelativePathOrOriginal(string? path)
    {
        if (String.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string basePath = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        string fullPath = Path.GetFullPath(path);

        if (fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetRelativePath(basePath, fullPath);
        }

        return fullPath;
    }
}
