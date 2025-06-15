using Microsoft.Extensions.Logging;
using Sqeeper.Config;
using Sqeeper.Core;
using ZLogger;

namespace Sqeeper.Command
{
    public class UpdateCommand
    {
        private readonly DownloadService _downloadService;
        private readonly ConfigBuilder _configBuilder;
        private readonly LinkService _linkService;
        private readonly ILogger<UpdateCommand> _logger;

        public UpdateCommand(ConfigBuilder configBuilder,
            LinkService linkService,
            DownloadService downloadService,
            ILogger<UpdateCommand> logger)
        {
            _configBuilder = configBuilder;
            _linkService = linkService;
            _logger = logger;
            _downloadService = downloadService;
        }

        /// <summary>
        ///     Runs through the config and updates all mentioned apps.
        /// </summary>
        /// <param name="name">-n, Name of the app to update.</param>
        public async Task Update(string? name = null)
        {
            var config =
                (name is null ? _configBuilder.IncludeApps() : _configBuilder.IncludeApp(name))
                .IncludeGroupDefaults().IncludeDefaults().Build();
            if (config.Length == 0)
                return;
            
            for (var i = 0; i < config.Length; i++)
            {
                var app = config.Get(i);
                if (!await UpdateApp(app))
                    _logger.ZLogError($"{app.Name} skipped.");
            }
        }

        private async Task<bool> UpdateApp(AppConfig app)
        {
            var link = await _linkService.TryGetDownloadLink(app.Url, app.SourceType, app.Query, app.AntiQuery, app.Version);
            if (link is null)
                return false;

            // DownloadFile(link, app.Path);
            
            return true;
        }
    }
}