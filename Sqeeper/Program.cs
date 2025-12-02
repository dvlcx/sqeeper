using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sqeeper.Command;
using Sqeeper.Config;
using Sqeeper.Core;
using Sqeeper.Core.Links;
using Sqeeper.Core.Links.Abstract;

namespace Sqeeper;

class Program
{
    public static string CachePath { get; } =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.cache/sqeeper/";
    private static readonly string _configPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.config/sqeeper/sqeeper.ini";

    static void Main(string[] args)
    {
        if (!File.Exists(_configPath))
        {
            //createdefault
        }

        var appConfigBuilder = new ConfigBuilder(
            new ConfigurationBuilder().AddIniFile(_configPath, false, false).Build());

        var services = new ServiceCollection();
        services.AddSingleton(appConfigBuilder);
        services.AddSingleton<HttpClientService>();

        services.AddTransient<ILinkStrategy, GitHubReleaseStrategy>();
        services.AddTransient<ILinkStrategy, GitLabReleaseStrategy>();
        services.AddTransient<ILinkStrategy, CodebergReleaseStrategy>();
        services.AddTransient<ILinkStrategy, DirectoryIndexStrategy>();
        services.AddTransient<ILinkStrategy, GitRepositoryStrategy>();

        services.AddSingleton<LinkService>();
        services.AddSingleton<DownloadService>();
        using var serviceProvider = services.BuildServiceProvider();
        ConsoleApp.ServiceProvider = serviceProvider;

        var app = ConsoleApp.Create();
        app.Add<UpdateCommand>();

        app.Run(args);
    }
}
