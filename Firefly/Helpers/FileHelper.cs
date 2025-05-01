using System;
using System.Diagnostics;
using System.IO;

namespace Firefly.Helpers;

public static class FileHelper
{
    public static void OpenFile(string path, bool relative = false)
    {
        if (relative)
        {
            path = Path.Combine(AppContext.BaseDirectory, path);
        }

        Process.Start("explorer.exe", $"\"{path}\"");
    }

    public static void OpenFolder(string path, bool relative = false)
    {
        if (relative)
        {
            path = Path.Combine(AppContext.BaseDirectory, path);
        }

        if (File.Exists(path))
        {
            Process.Start("explorer.exe", $"/select, \"{path}\"");
        }
        else
        {
            OpenFile(path);
        }
    }
}
