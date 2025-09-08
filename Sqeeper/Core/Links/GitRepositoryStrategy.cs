using Sqeeper.Config.Models;
using Sqeeper.Core.Links.Abstract;

namespace Sqeeper.Core.Links;

public class GitRepositoryStrategy : ILinkStrategy
{
    public bool CanHandle(UpdateSource sourceType)
        => sourceType == UpdateSource.GitRepository;
    
    public Task<List<string>> GetLinksAsync(ILinkConfig linkConfig)
        => Task.FromResult(linkConfig.Url.EndsWith(".git") ? [linkConfig.Url] : new List<string>());
}