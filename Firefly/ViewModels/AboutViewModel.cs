using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Firefly.Common;
using Firefly.Models;

namespace Firefly.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    #region Properties

    public ObservableRangeCollection<PackageDependency> AppDependencies { get; } = [];

    public string ReleasedLongTime => ReleasedTime.ToString("yyyy/MM/dd dddd tt h:mm:ss.fff");

    public DateTime ReleasedTime => File.GetLastWriteTime(GetType().Assembly.Location);

    #endregion Properties

    #region Commands

    [RelayCommand]
    private async Task LoadedAsync()
    {
        await foreach (var package in GetAppDependenciesAsync())
        {
            AppDependencies.Add(package);
        }
    }

    #endregion Commands

    #region Methods

    public static async IAsyncEnumerable<PackageDependency> GetAppDependenciesAsync()
    {
        const string depsJsonPath = $"{App.AppName}.deps.json";

        if (!File.Exists(depsJsonPath))
        {
            throw new FileNotFoundException("Could not find deps.json file.", depsJsonPath);
        }

        string json = await File.ReadAllTextAsync(depsJsonPath);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("targets", out var targetsElement))
        {
            foreach (var framework in targetsElement.EnumerateObject())
            {
                foreach (var package in framework.Value.EnumerateObject())
                {
                    if (package.Value.TryGetProperty("dependencies", out var dependencies))
                    {
                        foreach (var dep in dependencies.EnumerateObject())
                        {
                            yield return new PackageDependency(dep.Name, dep.Value.GetString() ?? "Unknow");
                        }

                        yield break;
                    }
                }
            }
        }
    }

    #endregion Methods
}
