using System;
using System.Threading.Tasks;

using Firefly.Helpers;

namespace Firefly.Models;

public static class GeoResources
{
    public static readonly Lazy<Task<string>> ChinaLocationsLazy = new(Task.Run(ReadChinaLocationsAsync));

    public static readonly Lazy<Task<string>> CountriesAndRegionsLazy = new(Task.Run(ReadCountriesAndRegionsAsync));

    public static string ChinaLocations { get; private set; } = "";

    public static string CountriesAndRegions { get; private set; } = "";

    public static async Task ReadAsync()
    {
        CountriesAndRegions = await CountriesAndRegionsLazy.Value;
        ChinaLocations = await ChinaLocationsLazy.Value;
    }

    private static async Task<string> ReadChinaLocationsAsync()
    {
        return await ResourceHelper.ReadAllTextAsync("Resources/Data/中国省市县区列表.txt");
    }

    private static async Task<string> ReadCountriesAndRegionsAsync()
    {
        return await ResourceHelper.ReadAllTextAsync("Resources/Data/国家地区列表.txt");
    }
}
