using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqeeper.Config;
using Sqeeper.Config.Models;
using Sqeeper.Core.Links;
using Sqeeper.Core.Links.Abstract;
using ZLogger;

namespace Sqeeper.Core;

public class LinkService
{
    private readonly HttpClient _client;
    private readonly ILogger<LinkService> _logger;
    private readonly IEnumerable<ILinkStrategy> _strategies;
    
    public LinkService(HttpClientService httpClientService, ILogger<LinkService> logger, IEnumerable<ILinkStrategy> strategies)
    {
        _client = httpClientService.Instance;
        _logger = logger;
        _strategies = strategies;
    }

    public async Task<string?> TryGetDownloadLink(ILinkConfig linkConfig)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(linkConfig.SourceType));
        if (strategy == null)
        {
            _logger.LogError($"No strategy found for source type: {linkConfig.SourceType}");
            return null;
        }
        
        var links = await strategy.GetLinksAsync(linkConfig);
        
        if (links.Count == 0)
        {
            _logger.ZLogError($"No suitable links found at: {linkConfig.Url}.");
            return null;
        }
        else if (links.Count > 1)
        {
            _logger.ZLogError($"More than one link found for {linkConfig.Url}.");
            return null;
        }
        return links[0];
    }
}