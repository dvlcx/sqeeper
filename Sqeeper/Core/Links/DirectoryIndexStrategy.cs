using Sqeeper.Config.Models;
using Sqeeper.Core.Links.Abstract;

namespace Sqeeper.Core.Links;

public class DirectoryIndexStrategy : ILinkStrategy
{
    protected readonly HttpClient _client;

    public DirectoryIndexStrategy(HttpClientService httpClientService)
    {
        _client = httpClientService.Instance;
    }
    
    public bool CanHandle(UpdateSource sourceType) => 
        sourceType == UpdateSource.DirectoryIndex;

    public async Task<List<string>> GetLinksAsync(ILinkConfig linkConfig)
    {
        var links = await this.GetLinksDirectoryIndexAsync(linkConfig.Url);
        links = links
            .Where(l => Utils.IsNewerVersion(linkConfig.Version, Utils.TryExtractVersion(l)!))
            .Where(l => linkConfig.Query.All(l.Contains) && !linkConfig.AntiQuery.All(l.Contains))
            .ToList();
        return links;
    }
    
    private async Task<List<string>> GetLinksDirectoryIndexAsync(string url)
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
            lines = await GetLinksDirectoryIndexAsync(url + lines.First());
        
        return lines.ToList();
    }
}