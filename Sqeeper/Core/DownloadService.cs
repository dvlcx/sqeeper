using System.Diagnostics;
using Sqeeper.Config.Models;
using static Sqeeper.Program;

namespace Sqeeper.Core;

public class DownloadService
{
    private readonly HttpClient _client;

    public DownloadService(HttpClientService httpClientService)
    {
        _client = httpClientService.Instance;
    }

    public async Task<bool> TryDownloadUpdate(AppConfig appConfig) =>
        appConfig.SourceType == UpdateSource.GitRepository ?
            await TryDownloadGitUpdate(appConfig.Path, appConfig.Url) :
            await TryDownloadFile(appConfig.Url, appConfig.Name);

    private async Task<bool> TryDownloadGitUpdate(string path, string url)
    {
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
            Console.WriteLine($"git pull failed at {path}. Not a git repo. Will try to git clone."); ;
            psi.Arguments = $"-C {path} clone {url}";
            using var process2 = Process.Start(psi);
            if (process.ExitCode != 0)
            {
                Console.WriteLine($"git clone failed at {path}.");
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
            Console.WriteLine($"{url} is not a file link.");
            return false;
        }
        using (var fs = new FileStream(CachePath + fileName, FileMode.CreateNew))
            await response.Content.CopyToAsync(fs);
        return true;
    }
}
