using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Firefly.Common;
using Firefly.Models;

using HandyControl.Controls;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

using HcMessageBox = HandyControl.Controls.MessageBox;

namespace Firefly.ViewModels;

public partial class CccfDbMergeViewModel : ObservableObject
{
    #region Fields

    public const string Title = "Firefly (萤火虫): 合并本地数据库";

    #endregion Fields

    #region Properties

    public bool CanMerge => MasterDbInfo is not null && SourceDbInfos.Count > 0;

    [ObservableProperty]
    public partial FireTaskStatus FireTaskStatus { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MergeCommand))]
    public partial DbInfo? MasterDbInfo { get; private set; }

    public ObservableRangeCollection<DbInfo> SourceDbInfos { get; } = [];

    public int SourceDbInfosCount => SourceDbInfos.Count;

    [ObservableProperty]
    public partial bool Topmost { get; set; } = true;

    #endregion Properties

    #region Constructors & Recipients

    public CccfDbMergeViewModel()
    {
        SourceDbInfos.CollectionChanged += SourceDbInfos_CollectionChanged;
    }

    #endregion Constructors & Recipients

    #region Commands

    [RelayCommand]
    private async Task AddMasterDbAsync()
    {
        var dialog = new OpenFileDialog {
            AddToRecent = true,
            CheckFileExists = true,
            DefaultExt = ".db",
            Filter = "SQLite 数据库|*.db;*.sqlite;*.sqlite3;*.s3db;*.sl3|所有文件|*.*",
            Title = "添加主数据库",
        };

        if (dialog.ShowDialog() is true)
        {
            var used = SourceDbInfos.FirstOrDefault(x => x.Path == dialog.FileName);

            if (used is not null)
            {
                var result = HcMessageBox.Show(
                     $"{dialog.FileName} 已作为数据源使用，是否将其更改为主数据库？",
                     App.AppName,
                     MessageBoxButton.YesNo,
                     MessageBoxImage.Question,
                     MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    SourceDbInfos.Remove(used);

                    MasterDbInfo = new DbInfo(dialog.FileName);

                    try
                    {
                        await MasterDbInfo.GetTotalRecordsAsync();
                    }
                    catch (Exception)
                    {
                        MasterDbInfo = null;

                        throw;
                    }
                }
            }
            else
            {
                MasterDbInfo = new DbInfo(dialog.FileName);

                try
                {
                    await MasterDbInfo.GetTotalRecordsAsync();
                }
                catch (Exception)
                {
                    MasterDbInfo = null;

                    throw;
                }
            }
        }
    }

    [RelayCommand]
    private async Task AddSourceDbAsync()
    {
        var dialog = new OpenFileDialog {
            AddToRecent = true,
            CheckFileExists = true,
            DefaultExt = ".db",
            Filter = "SQLite 数据库|*.db;*.sqlite;*.sqlite3;*.s3db;*.sl3|所有文件|*.*",
            Title = "添加数据源",
            Multiselect = true
        };

        int duplicate = 0;

        if (dialog.ShowDialog() is true)
        {
            var infos = dialog.FileNames.Select(x => new DbInfo(x));

            foreach (var info in infos)
            {
                if (!SourceDbInfos.Any(x => x.Path == info.Path) && info.Path != MasterDbInfo?.Path)
                {
                    SourceDbInfos.Add(info);

                    try
                    {
                        await info.GetTotalRecordsAsync();
                    }
                    catch (Exception)
                    {
                        SourceDbInfos.Remove(info);

                        throw;
                    }
                }
                else
                {
                    duplicate++;
                }
            }

            if (duplicate > 0)
            {
                HcMessageBox.Show(
                    $"检测到 {duplicate} 个重复的数据库，已自动去除重复项。",
                    App.AppName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }

    [RelayCommand]
    private void ClearSourceDb()
    {
        SourceDbInfos.Clear();
    }

    [RelayCommand]
    private void Closing(CancelEventArgs e)
    {
        if (MergeCommand.IsRunning)
        {
            var result = HcMessageBox.Show(
                "正在合并本地数据库，确定关闭？",
                App.AppName,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                MergeCancelCommand.Execute(null);
            }
            else
            {
                e.Cancel = true;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanMerge), IncludeCancelCommand = true)]
    private async Task MergeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = HcMessageBox.Show(
                  "确定合并数据库？\n\n* 合并前请务必备份主数据库！！！",
                  App.AppName,
                  MessageBoxButton.OKCancel,
                  MessageBoxImage.Warning);

            if (result != MessageBoxResult.OK)
            {
                return;
            }

            ResetProgressStatus();
            MasterDbInfo!.IsMerging = true;
            FireTaskStatus = FireTaskStatus.Normal;

            for (int i = 0; i < SourceDbInfosCount; i++)
            {
                var info = SourceDbInfos[i];

                try
                {
                    info.IsMerging = true;
                    info.FireTaskStatus = FireTaskStatus.Normal;

                    var masterBuilder = new DbContextOptionsBuilder<CccfDbContext>();
                    masterBuilder.UseSqlite($"Data Source={MasterDbInfo.Path}");
                    masterBuilder.UseLazyLoadingProxies();
                    using var masterDbContext = new CccfDbContext(masterBuilder.Options);

                    var builder = new DbContextOptionsBuilder<CccfDbContext>();
                    masterBuilder.UseSqlite($"Data Source={info.Path}");
                    masterBuilder.UseLazyLoadingProxies();
                    using var dbContext = new CccfDbContext(masterBuilder.Options);

                    await Task.Run(async () => {
                        int count = await dbContext.Products.CountAsync(cancellationToken);
                        var cccfs = dbContext.Products.AsQueryable();
                        const int rows = 10000;

                        for (int r = 0; r < count; r += rows)
                        {
                            int upsert = await masterDbContext.Products.UpsertRange(cccfs.Skip(r).Take(rows)).RunAsync(cancellationToken);
                            info.MergeProgress = (double)(r + upsert) / count;
                            MasterDbInfo.MergeProgress = (i + 1) / SourceDbInfosCount * info.MergeProgress;

                            await Task.Delay(10, cancellationToken);
                        }

                        info.MergeProgress = 1d;
                    }, cancellationToken);

                    info.FireTaskStatus = FireTaskStatus.Completed;
                    info.IsMerging = false;
                }
                catch
                {
                    info.FireTaskStatus = FireTaskStatus.Error;
                    await Task.Delay(1000, CancellationToken.None);
                    info.IsMerging = false;

                    throw;
                }
            }

            MasterDbInfo.MergeProgress = 1d;
            FireTaskStatus = FireTaskStatus.Completed;
            MasterDbInfo.Refresh();
            HcMessageBox.Info("合并完成。", App.AppName);
        }
        catch (TaskCanceledException)
        {
            FireTaskStatus = FireTaskStatus.Cancelled;
            Growl.Info("已中止合并。");
        }
        catch (Exception ex)
        {
            FireTaskStatus = FireTaskStatus.Error;
            HcMessageBox.Error($"合并发生错误。\n\n{ex}", App.AppName);

            throw;
        }
        finally
        {
            MasterDbInfo!.IsMerging = false;
            await MasterDbInfo.GetTotalRecordsAsync();
        }
    }

    [RelayCommand]
    private void RemoveSourceDb(DbInfo remove)
    {
        SourceDbInfos.Remove(remove);
    }

    #endregion Commands

    #region Methods

    private void ResetProgressStatus()
    {
        foreach (var info in SourceDbInfos)
        {
            info.MergeProgress = 0;
        }

        if (MasterDbInfo is not null)
        {
            MasterDbInfo.MergeProgress = 0;
        }
    }

    #endregion Methods

    #region Event Handlers

    private void SourceDbInfos_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SourceDbInfosCount));
        MergeCommand.NotifyCanExecuteChanged();
    }

    #endregion Event Handlers
}
