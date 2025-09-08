using Sqeeper.Config.Models;

namespace Sqeeper.Core.Links.Abstract;

public interface ILinkStrategy
{
    bool CanHandle(UpdateSource sourceType);
    Task<List<string>> GetLinksAsync(ILinkConfig linkConfig);
}