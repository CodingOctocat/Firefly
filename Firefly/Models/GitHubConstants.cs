namespace Firefly.Models;

public static class GitHubConstants
{
    public const string DiscussionsUrl = $"{RepositoryUrl}/discussions";

    public const string IssuesUrl = $"{RepositoryUrl}/issues";

    public const string LatestReleaseUrl = $"{ReleasesUrl}/latest";

    public const string LicenseUrl = $"{RepositoryUrl}/blob/master/LICENSE";

    public const string MyGitHubUserName = "CodingOctocat";

    public const string ReleasesUrl = $"{RepositoryUrl}/releases";

    public const string RepositoryUrl = $"https://github.com/{MyGitHubUserName}/{App.AppName}";

    public const string WikiUrl = $"{RepositoryUrl}/wiki";
}
