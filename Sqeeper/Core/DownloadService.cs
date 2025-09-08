using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Sqeeper.Config;
using ZLogger;
using static Sqeeper.Program;

namespace Sqeeper.Core;

public class DownloadService
{
    private readonly HttpClient _client;
    private readonly ILogger<DownloadService> _logger;

    public DownloadService(HttpClientService httpClientService, ILogger<DownloadService> logger)
    {
        _client = httpClientService.Instance;
        _logger = logger;
    }

    public async Task<bool> TryDownloadUpdate(AppConfig appConfig) =>
        appConfig.SourceType == UpdateSource.GitRepository ?
            await TryDownloadGitUpdate(appConfig.Path, appConfig.Url) :
            await TryDownloadFile(appConfig.Url, appConfig.Name);

    private async Task<bool> TryDownloadGitUpdate(string path, string url)
    {
        string command = "git";

        var psi = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            FileName = @"/usr/bin/git",
            Arguments = $"-C {path} pull",
        };
        using var process = Process.Start(psi);
        if (process is null)
            return false;
        await process.WaitForExitAsync();
        if (process.ExitCode == 128)
        {
            _logger.ZLogWarning($"git pull failed at {path}. Not a git repo. Will try to git clone."); ;
            psi.Arguments = $"-C {path} clone {url}";
            using var process2 = Process.Start(psi);
            if (process.ExitCode != 0)
            {
                _logger.ZLogError($"git clone failed at {path}.");
                return false;
            }
            else return true;
        }
        else if (process.ExitCode != 0) return false;
        else return true;
    }

    private async Task<bool> TryDownloadFile(string url, string name)
    {
        var response = await _client.GetAsync(url);
        string? fileName = response.Content.Headers.ContentDisposition?.FileName;
        if (fileName is null)
        {
            _logger.ZLogError($"{url} is not a file link.");
            return false;
        }
        using (var fs = new FileStream(CachePath + fileName, FileMode.CreateNew))
            await response.Content.CopyToAsync(fs);
        return true;
    }
}
