using System;
using System.Threading.Tasks;

using Firefly.Helpers;

namespace Firefly.Models;

public static class CccfCatalogyResources
{
    public static readonly Lazy<Task<string>> CccfCatalogsLazy = new(Task.Run(ReadCccfCatalogsAsync));

    public static string CccfCatalogs { get; private set; } = "";

    public static async Task ReadAsync()
    {
        CccfCatalogs = await CccfCatalogsLazy.Value;
    }

    private static async Task<string> ReadCccfCatalogsAsync()
    {
        return await ResourceHelper.ReadAllTextAsync("Resources/Data/消防产品目录表.txt");
    }
}
