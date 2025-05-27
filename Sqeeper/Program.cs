using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Sqeeper;

class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();

        using var serviceProvider = services.BuildServiceProvider();
        ConsoleApp.ServiceProvider = serviceProvider;

        var app = ConsoleApp.Create();
        app.Run(args);
    }
}