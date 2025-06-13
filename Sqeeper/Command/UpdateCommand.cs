using Microsoft.Extensions.Logging;
using Sqeeper.Config;
using Sqeeper.Core;
using ZLogger;

namespace Sqeeper.Command
{

    public class UpdateCommand(
        ConfigBuilder configBuilder,
        LinkService linkService,
        DownloadService downloadService,
        ILogger<UpdateCommand> logger)
    {
        private readonly DownloadService _downloadService = downloadService;

        /// <summary>
        ///     Runs through the config and updates all mentioned apps.
        /// </summary>
        /// <param name="name">-n, Name of the app to update.</param>
        public async Task Update(string? name = null)
        {
            var config =
                (name is null ? configBuilder.IncludeApps() : configBuilder.IncludeApp(name))
                .IncludeGroupDefaults().IncludeDefaults().Build();
            if (config.Length == 0)
                return;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("a", "s");
            
            for (var i = 0; i < config.Length; i++)
            {
                var app = config.Get(i);
                if (!await UpdateApp(app, httpClient))
                    logger.ZLogError($"{app.Name} skipped.");
            }
        }

        private async Task<bool> UpdateApp(AppConfig app, HttpClient httpClient)
        {
            var link = await linkService.TryGetDownloadLink(httpClient, app.Url, app.SourceType, app.Query, app.AntiQuery, app.Version);
            if (link is null)
                return false;

            // DownloadFile(link, app.Path);
            
            return true;
        }
    }
}