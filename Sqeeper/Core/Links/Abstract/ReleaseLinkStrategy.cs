using System.Text.Json;
using Sqeeper.Config.Models;

namespace Sqeeper.Core.Links.Abstract;

public abstract class ReleaseLinkStrategyBase : ILinkStrategy
{
    private readonly HttpClient _client;
    
    protected ReleaseLinkStrategyBase(HttpClientService httpClientService)
    {
        _client = httpClientService.Instance;
    }
    
    public abstract bool CanHandle(UpdateSource sourceType);
    
    public async Task<List<string>> GetLinksAsync(ILinkConfig linkConfig)
    {
        var (assetsPath, urlPropertyName) = GetJsonPaths();
        return await GetReleaseLinks(linkConfig, assetsPath, urlPropertyName);
    }
    
    protected abstract (string assetsPath, string urlPropertyName) GetJsonPaths();
    
    protected async Task<List<string>> GetReleaseLinks(ILinkConfig linkConfig, string assetsPath, string urlPropertyName)
    {
        var response = await _client.GetAsync(linkConfig.Url);
        var text = await response.Content.ReadAsStringAsync();
        
        List<string> downloadUrls = [];
        using (JsonDocument doc = JsonDocument.Parse(text))
        {
            if (linkConfig.VersionPropertyName != null)
            {
                doc.RootElement.TryGetProperty(linkConfig.VersionPropertyName, out var versionProperty);
                var version = versionProperty.GetRawText().Trim('"');
                if (!Utils.VersionValidate(linkConfig.Version, version))
                    return downloadUrls;
            }
            
            var links = assetsPath.Split('.').Aggregate(doc.RootElement, (current, pathPart) => current.GetProperty(pathPart));
            foreach (JsonElement linkElement in links.EnumerateArray())
            {
                string? prop = null;
                string? link = null;
                if (linkConfig.QueryablePropertyName != null)
                {
                    linkElement.TryGetProperty(linkConfig.QueryablePropertyName, out JsonElement queryableProperty);
                    prop = queryableProperty.GetRawText().Trim('"');
                }
                
                if (linkElement.TryGetProperty(urlPropertyName, out JsonElement urlElement))
                    link = urlElement.GetRawText().Trim('"');
                
                if (link == null) continue;
                if (linkConfig.VersionPropertyName == null && !Utils.VersionValidate(linkConfig.Version, link))
                    continue;
                if (Utils.QueryValidate(prop ?? link, linkConfig.Query, linkConfig.AntiQuery))
                    downloadUrls.Add(link);
            }
        }
        return downloadUrls;
    }
}