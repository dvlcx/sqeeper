using Sqeeper.Config.Models;
using Sqeeper.Core.Links.Abstract;

namespace Sqeeper.Core.Links;

public class GitHubReleaseStrategy : ReleaseLinkStrategyBase
{
    public GitHubReleaseStrategy(HttpClientService httpClientService) : base(httpClientService)
    {
    }

    public override bool CanHandle(UpdateSource sourceType) =>
        sourceType == UpdateSource.GitHubRelease;

    protected override (string assetsPath, string urlPropertyName) GetJsonPaths() =>
        ("assets", "browser_download_url");
}