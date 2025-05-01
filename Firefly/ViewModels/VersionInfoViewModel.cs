using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Firefly.Extensions;
using Firefly.Helpers;
using Firefly.Models;
using Firefly.Models.Responses;
using Firefly.Properties;

using HandyControl.Controls;

using Octokit;

namespace Firefly.ViewModels;

public partial class VersionInfoViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CheckUpdateErrorMessage))]
    [NotifyPropertyChangedFor(nameof(NewVersionAvailableToolTip))]
    public partial Exception? CheckUpdateError { get; private set; }

    public string CheckUpdateErrorMessage
    {
        get
        {
            if (CheckUpdateError is HttpRequestException ex && ex.StatusCode is not null)
            {
                var status = (FriendlyHttpStatusCode)ex.StatusCode;
                string desc = status.GetDescription();

                return $"HTTP 响应错误({(int)ex.StatusCode} {ex.StatusCode}):\n{desc}";
            }

            if (CheckUpdateError is RateLimitExceededException ex2)
            {
                return $"请求过于频繁。请稍后再试。(-{ex2.GetRetryAfterTimeSpan().TotalSeconds:F2}s)\n{ex2.Message}";
            }

            return CheckUpdateError?.Message ?? "";
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewVersionAvailableToolTip))]
    public partial bool HasNewVersion { get; private set; }

    public string NewVersionAvailableToolTip
    {
        get
        {
            if (CheckUpdateError is not null)
            {
                return $"检查更新失败\n\n{CheckUpdateErrorMessage}";
            }

            if (HasNewVersion)
            {
                return "新版本可用: {0}\n\n{1} -> {2}".Format(
                    ReleaseInfo?.Name ?? "<?.?.?>",
                    App.VersionString,
                    ReleaseInfo?.TagName ?? "<?.?.?>");
            }
            else
            {
                return "你使用的是最新版本";
            }
        }
    }

    [ObservableProperty]
    public partial Release? ReleaseInfo { get; private set; }

    #endregion Properties

    #region Commands

    [RelayCommand]
    private async Task CheckUpdateAsync(bool showGrowl)
    {
        if (ReleaseInfo is null)
        {
            await GetReleaseInfoAsync();
        }

        if (!showGrowl)
        {
            return;
        }

        ShowReleaseResult();
    }

    [RelayCommand]
    private async Task LoadedAsync()
    {
        bool showGrowl = false;

        if (DateTime.Now - Settings.Default.LastCheckedTime > TimeSpan.FromDays(7))
        {
            showGrowl = true;
        }

        await CheckUpdateCommand.ExecuteAsync(showGrowl);
    }

    #endregion Commands

    #region Methods

    private async Task GetReleaseInfoAsync()
    {
        try
        {
            var github = new GitHubClient(new ProductHeaderValue($"{GitHubConstants.MyGitHubUserName}-{App.AppName}", App.VersionString));
            ReleaseInfo = await github.Repository.Release.GetLatest(GitHubConstants.MyGitHubUserName, App.AppName);

            if (Version.Parse(ReleaseInfo.TagName.TrimStart('v')) > App.Version)
            {
                HasNewVersion = true;
            }
        }
        catch (Exception ex)
        {
            ReleaseInfo = null;
            HasNewVersion = false;
            CheckUpdateError = ex;
        }
        finally
        {
            Settings.Default.LastCheckedTime = DateTime.Now;
        }
    }

    private void ShowReleaseNote()
    {
        ArgumentNullException.ThrowIfNull(ReleaseInfo);

        string body = ReleaseNoteFormatRegex().Replace(ReleaseInfo.Body, "• ");
        body = String.IsNullOrWhiteSpace(body) ? "<null>" : body;

        Growl.Ask(new() {
            ConfirmStr = "跳转到 GitHub",
            CancelStr = "取消",
            Message = $"""
            最新版本: {ReleaseInfo.Name}
            {App.VersionString} -> {ReleaseInfo.TagName}
            {ReleaseInfo.PublishedAt?.ToLocalTime():yyyy/MM/dd HH:mm:ss 'GMT'z}

            What's Changed
            {body}
            """,
            ActionBeforeClose = (isConfirm) => {
                if (isConfirm)
                {
                    UriHelper.OpenUri(ReleaseInfo.HtmlUrl);
                }

                return true;
            }
        });
    }

    private void ShowReleaseResult()
    {
        if (CheckUpdateError is not null)
        {
            string innerExMsg = CheckUpdateError.InnerException is null ? "" : $"\n\n{new string('-', 24)}\n{CheckUpdateError.InnerException.Message}";
            Growl.Error($"检查更新失败\n\n{CheckUpdateErrorMessage}{innerExMsg}");

            return;
        }

        if (!HasNewVersion)
        {
            Growl.Info("你使用的是最新版本");

            return;
        }

        ShowReleaseNote();
    }

    #endregion Methods

    #region Regex

    [GeneratedRegex(@"^\-\s", RegexOptions.Multiline)]
    private static partial Regex ReleaseNoteFormatRegex();

    #endregion Regex
}
