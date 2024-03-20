using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Services;

namespace SonarProfileSwitcher
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create a host builder
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register services with dependency injection
                    services.AddSingleton<IHostedService, MainProcess>();
                    services.AddTransient<ISteelSeriesSonarService, SteelSeriesSonarService>();
                    services.AddTransient<IFileServices, FileServices>();
                    services.AddTransient<IProfileServices, ProfileServices>();
                    services.AddTransient<IProcessServices, ProcessServices>();
                    services.AddTransient<ISmartScreenServices, SmartScreenServices>();
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddFile
                    }
                })
                .Build();

            // Resolve dependencies from the container
            var mainProcess = host.Services.GetRequiredService<IHostedService>();

            await host.RunAsync();
        }
    }
}
