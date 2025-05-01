using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.EntityFrameworkCore;

namespace Firefly.Models;

public partial class DbInfo : ObservableObject
{
    public DateTime CreationTime { get; private set; }

    public string FileName { get; private set; }

    [ObservableProperty]
    public partial FireTaskStatus FireTaskStatus { get; set; }

    [ObservableProperty]
    public partial bool IsMerging { get; set; }

    [ObservableProperty]
    public partial double MergeProgress { get; set; }

    public DateTime ModifiedTime { get; private set; }

    public string Path { get; }

    [ObservableProperty]
    public partial int TotalRecords { get; private set; }

    public DbInfo(string path)
    {
        Path = path;

        var fi = new FileInfo(path);
        FileName = fi.Name;
        CreationTime = fi.CreationTime;
        ModifiedTime = fi.LastWriteTime;
    }

    public async Task GetTotalRecordsAsync()
    {
        TotalRecords = -1;

        var builder = new DbContextOptionsBuilder<CccfDbContext>();
        builder.UseSqlite($"Data Source={Path}");
        builder.UseLazyLoadingProxies();
        using var dbContext = new CccfDbContext(builder.Options);

        TotalRecords = await Task.Run(() => dbContext.Products.CountAsync());
    }

    public void Refresh()
    {
        try
        {
            var fi = new FileInfo(Path);
            FileName = fi.Name;
            OnPropertyChanged(nameof(FileName));
            CreationTime = fi.CreationTime;
            OnPropertyChanged(nameof(CreationTime));
            ModifiedTime = fi.LastWriteTime;
            OnPropertyChanged(nameof(ModifiedTime));
        }
        catch
        { }
    }
}

public class DbInfoEqualityComparer : EqualityComparer<DbInfo>
{
    public static readonly DbInfoEqualityComparer Instance = new();

    public override bool Equals(DbInfo? x, DbInfo? y)
    {
        return x?.Path == y?.Path;
    }

    public override int GetHashCode([DisallowNull] DbInfo obj)
    {
        return obj.Path.GetHashCode();
    }
}
