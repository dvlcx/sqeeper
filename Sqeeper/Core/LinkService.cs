using Sqeeper.Config.Models;
using Sqeeper.Core.Links.Abstract;

namespace Sqeeper.Core;

public class LinkService
{
    private readonly HttpClient _client;
    private readonly IEnumerable<ILinkStrategy> _strategies;

    public LinkService(HttpClientService httpClientService, IEnumerable<ILinkStrategy> strategies)
    {
        _client = httpClientService.Instance;
        _strategies = strategies;
    }

    public async Task<string?> TryGetDownloadLink(ILinkConfig linkConfig)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(linkConfig.SourceType));
        if (strategy == null)
        {
            Console.WriteLine($"No strategy found for source type: {linkConfig.SourceType}");
            return null;
        }

        var links = await strategy.GetLinksAsync(linkConfig);

        if (links.Count == 0)
        {
            Console.WriteLine($"No suitable links found at: {linkConfig.Url}.");
            return null;
        }
        else if (links.Count > 1)
        {
            Console.WriteLine($"More than one link found for {linkConfig.Url}.");
            return null;
        }
        return links[0];
    }
}
