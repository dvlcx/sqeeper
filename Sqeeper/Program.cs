using System.Data;
using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sqeeper.Command;
using Sqeeper.Config;
using Sqeeper.Config.Models;
using Sqeeper.Core;
using ZLogger;

namespace Sqeeper;

class Program
{

    private static string _configPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.config/sqeeper/sqeeper.ini";
    static void Main(string[] args)
    {
        if (!File.Exists(_configPath))
        {

        }

        var appConfigBuilder = new ConfigBuilder(
            new ConfigurationBuilder().AddIniFile(_configPath, false, false).Build(),
            LoggerFactory.Create(logging => GetLoggingBuilder(logging)).CreateLogger<ConfigBuilder>());

        var services = new ServiceCollection();
        services.AddSingleton(appConfigBuilder);
        services.AddSingleton<UpdateService>();
        services.AddLogging(logger => GetLoggingBuilder(logger));
        using var serviceProvider = services.BuildServiceProvider();
        ConsoleApp.ServiceProvider = serviceProvider;

        var app = ConsoleApp.Create();
        app.Add<UpdateCommand>();

        app.Run(args);
    }

    private static ILoggingBuilder GetLoggingBuilder(ILoggingBuilder builder) =>
        builder.ClearProviders().SetMinimumLevel(LogLevel.Information).AddZLoggerConsole();
}