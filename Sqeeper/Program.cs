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

        ConsoleApp.Run(args, (string name) => Console.WriteLine($"Hello, {name}!"));
    }
}