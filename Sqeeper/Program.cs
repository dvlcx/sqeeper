using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sqeeper.Config;
using ZLogger;

namespace Sqeeper;

class Program
{
    private static string _configPath = "~/.config/sqeeper/sqeeper.ini";
    static void Main(string[] args)
    {
        if (!File.Exists(_configPath))
        {
        }

        List<AppConfig> appConfigs = new ConfigBuilder(
            new ConfigurationBuilder()
            .AddIniFile(_configPath, false, false)
            .Build(),
            LoggerFactory.Create(logging => GetLoggingBuilder(logging)).CreateLogger<ConfigBuilder>())
            .IncludeApps().IncludeDefaults().IncludeGroupDefaults().Build();

        var services = new ServiceCollection();
        services.AddSingleton(appConfigs);
        services.AddLogging(logger => GetLoggingBuilder(logger));
        services.AddSingleton<List<AppConfig>>(appConfigs);
        using var serviceProvider = services.BuildServiceProvider();
        ConsoleApp.ServiceProvider = serviceProvider;
        var app = ConsoleApp.Create();
        app.Run(args);
    }

    private static ILoggingBuilder GetLoggingBuilder(ILoggingBuilder builder) =>
        builder.ClearProviders().SetMinimumLevel(LogLevel.Information).AddZLoggerConsole();
}