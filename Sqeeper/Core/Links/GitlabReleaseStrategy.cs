using Sqeeper.Config.Models;
using Sqeeper.Core.Links.Abstract;

namespace Sqeeper.Core.Links;

public class GitLabReleaseStrategy : ReleaseLinkStrategyBase
{
    public GitLabReleaseStrategy(HttpClientService httpClientService) : base(httpClientService)
    {
    }
    
    public override bool CanHandle(UpdateSource sourceType) =>
        sourceType == UpdateSource.GitLabRelease;

    protected override (string assetsPath, string urlPropertyName) GetJsonPaths() => 
        ("assets.links", "url");
}