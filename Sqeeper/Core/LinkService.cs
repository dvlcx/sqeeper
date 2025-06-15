using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sqeeper.Config;
using ZLogger;

namespace Sqeeper.Core;

public class LinkService
{
    private readonly HttpClient _client;
    private readonly ILogger<LinkService> _logger;
    
    public LinkService(HttpClientService httpClientService, ILogger<LinkService> logger)
    {
        _client = httpClientService.Instance;
        _logger = logger;
    }

    public async Task<string?> TryGetDownloadLink(string url, UpdateSource source, string[] query, string[] antiQuery, string currentVersion)
    {
        var links = source switch {
            UpdateSource.GitHubRelease => await GetLinksGithubRelease(url),
            UpdateSource.GitLabRelease => await GetLinksGitlabRelease(url),
            UpdateSource.DirectoryIndex => await GetLinksDirectoryIndex(url),
            UpdateSource.GitRepository => [url],
            _ => [],
        };
        
        if (source == UpdateSource.GitRepository)
            if (links[0].EndsWith(".git")) return links[0];
            else
            {
                _logger.LogError($"Not a git url: {url}.");
                return null;
            }

        links = links
            .Where(l => Utils.IsNewerVersion(currentVersion, Utils.TryExtractVersion(l)!))
            .Where(l => query.All(l.Contains) && !antiQuery.All(l.Contains))
            .ToList();
        
        if (links.Count == 0)
        {
            _logger.ZLogError($"No links found at: {url}.");
            return null;
        }
        else if (links.Count > 1)
        {
            _logger.ZLogError($"More than one link found for {url}.");
            return null;
        }
        return links[0];
    }

    private Task<List<string>> GetLinksGithubRelease(string url) 
        => GetReleaseLinks(url, "assets", "browser_download_url");

    private Task<List<string>> GetLinksGitlabRelease(string url) 
        => GetReleaseLinks(url, "assets.sources", "url");
    
    private async Task<List<string>> GetLinksDirectoryIndex(string url)
    {
        var response = await _client.GetAsync(url);
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
        var text = await reader.ReadToEndAsync();
        
        Console.WriteLine(text);
        List<string> downloadUrls = [];
        IEnumerable<string> lines = text.Split('\n');
        
        //find hrefs
        lines = lines.Select(Utils.TryExtractHref).Where(x => !string.IsNullOrEmpty(x))!;
        
        //find with max versions
        //if folder - supposed to be solo (cuz why would create folders with same versions)
        //if it is files - should be one or more (cuz platforms/extensions/etc)
        lines = Utils.GetLinesWithMaxVersions(lines.ToArray());
        
        //zero lines zero problems
        if (!lines.Any())
            return [];
        
        //more than one -- files
        if (lines.Count() > 1)
           return lines.ToList();
        if (lines.Count() == 1 && lines.First().EndsWith('/'))
        {
            lines = await GetLinksDirectoryIndex(url + lines.First());
        }
        
        return lines.ToList();;
    }
    
    private async Task<List<string>> GetReleaseLinks(string url, string assetsPath, string urlPropertyName)
    {
        var text = await GetTextFromUrl(url);

        List<string> downloadUrls = [];
        using (JsonDocument doc = JsonDocument.Parse(text))
        {
            var links = assetsPath.Split('.').Aggregate(doc.RootElement, (current, pathPart) => current.GetProperty(pathPart));
            foreach (JsonElement link in links.EnumerateArray())
                if (link.TryGetProperty(urlPropertyName, out JsonElement urlElement))
                    downloadUrls.Add(urlElement.GetRawText().Trim('"'));
        }
        return downloadUrls;
    }

    private async Task<string> GetTextFromUrl(string url)
    {
        var response = await _client.GetAsync(url);
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
        return await reader.ReadToEndAsync(); 
    }
}