using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sqeeper.Config;
using Sqeeper.Config.Models;

namespace Sqeeper.Core
{
    public class UpdateService
    {
        private ConfigBuilder _configBuilder;
        private ILogger<UpdateService> _logger;

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
                    await UpdateApp(app, httpClient);
                }
        }

        private async Task UpdateApp(AppConfig app, HttpClient httpClient)
        {
            var link = await TryGetDownloadLink(httpClient, app.Url, app.Query);
        }

        private async Task<string?> TryGetDownloadLink(HttpClient httpClient, string url, string[]? query)
        {
            var response = await httpClient.GetAsync(url);
            using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            {
                // Write the output.
                Console.WriteLine(await reader.ReadToEndAsync());
            }
            return "";
        }

        private bool DownloadFile(string url, string path)
        {
            return false;
        }

        private bool RunPostScript(string script, string path)
        {
            return false;
        }
    }
}