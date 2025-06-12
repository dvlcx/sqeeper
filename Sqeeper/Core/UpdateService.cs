using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sqeeper.Config;
using Sqeeper.Core.Utils;
using ZLogger;

namespace Sqeeper.Core
{
    public class UpdateService
    {
        private readonly ConfigBuilder _configBuilder;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(ConfigBuilder configBuilder, ILogger<UpdateService> logger)
        {
            _configBuilder = configBuilder;
            _logger = logger;
        }

        public async Task Update(string? name)
        {
            var config =
            (name is null ? _configBuilder.IncludeApps() : _configBuilder.IncludeApp(name))
            .IncludeGroupDefaults().IncludeDefaults().Build();

            if (config.Length == 0)
                return;

            using var httpClient = new HttpClient();
            for (int i = 0; i < config.Length; i++)
            {
                var app = config.Get(i);
                if (!await UpdateApp(app, httpClient))
                    _logger.ZLogError($"{app.Name} skipped.");
            }
        }

        private async Task<bool> UpdateApp(AppConfig app, HttpClient httpClient)
        {
            var link = await TryGetDownloadLink(httpClient, app.Url, app.Query, app.Version, app.IsGithub);
            if (link is null)
                return false;

            DownloadFile(link, app.Path);
            
            return true;
        }

        private async Task<string?> TryGetDownloadLink(HttpClient httpClient, string url, string[] query, string currentVersion, bool isGithub)
        {
            var links = isGithub ?
                await GetLinksGithub(httpClient, url) : 
                await GetLinks(httpClient, url);
            
            links = links
                .Where(l => Utils.Utils.IsNewerVersion(currentVersion, Utils.Utils.TryExtractVersion(l)!))
                .Where(l => query.All(l.Contains))
                .ToList();
            
            if (links.Count == 0)
            {
                _logger.ZLogError($"No links found at {url}.");
                return null;
            }
            else if (links.Count > 1)
            {
                _logger.ZLogError($"More than one link found for {url}.");
                return null;
            }
            return links[0];
        }

        private bool DownloadFile(string url, string path)
        {
            return false;
        }

        private bool RunPostScript(string script, string path)
        {
            return false;
        }

        private async Task<List<string>> GetLinksGithub(HttpClient httpClient, string url)
        {
            httpClient.DefaultRequestHeaders.Add("user-agent", "sqeeper");
            var response = await httpClient.GetAsync(url);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var text = await reader.ReadToEndAsync();
            List<string> downloadUrls = [];
            using (JsonDocument doc = JsonDocument.Parse(text))
            {
                JsonElement assets = doc.RootElement.GetProperty("assets");
    
                foreach (JsonElement asset in assets.EnumerateArray())
                    if (asset.TryGetProperty("browser_download_url", out JsonElement urlElement))
                        downloadUrls.Add(urlElement.GetRawText().Trim('"'));
            }
            return downloadUrls;
        }
        
        private async Task<List<string>> GetLinks(HttpClient httpClient, string url)
        {
            httpClient.DefaultRequestHeaders.Add("user-agent", "sqeeper");
            var response = await httpClient.GetAsync(url);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var text = await reader.ReadToEndAsync();
            
            Console.WriteLine(text);
            List<string> downloadUrls = [];
            IEnumerable<string> lines = text.Split('\n');
            
            //find hrefs
            lines = lines.Select(Utils.Utils.TryExtractHref).Where(x => !string.IsNullOrEmpty(x))!;
            
            //find with max versions
            //if folder - supposed to be solo (cuz why would create folders with same versions)
            //if it is files - should be one or more (cuz platforms/extensions/etc)
            lines = Utils.Utils.GetLinesWithMaxVersions(lines.ToArray());
            
            //zero lines zero problems
            if (!lines.Any())
                return [];
            
            //more than one -- files
            if (lines.Count() > 1)
               return lines.ToList();
            if (lines.Count() == 1 && lines.First().EndsWith('/'))
            {
                lines = await GetLinks(httpClient, url + lines.First());
            }
            
            return lines.ToList();;
        }
    }
}