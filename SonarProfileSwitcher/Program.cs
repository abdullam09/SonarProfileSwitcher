using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Services;
using Serilog;
using Serilog.Formatting.Compact;
using System.Runtime.InteropServices;

namespace SonarProfileSwitcher
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(new CompactJsonFormatter(), @"F:\Sonar Auto Switch\logs\log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Create a host builder
            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    // Register services with dependency injection
                    services.AddSingleton<IHostedService, MainProcess>();
                    services.AddTransient<ISteelSeriesSonarService, SteelSeriesSonarService>();
                    services.AddTransient<IFileServices, FileServices>();
                    services.AddTransient<IProfileServices, ProfileServices>();
                    services.AddTransient<IProcessServices, ProcessServices>();
                    services.AddTransient<ISmartScreenServices, SmartScreenServices>();
                    services.AddTransient<IKeyboardLayoutService, KeyboardLayoutService>();
                })
                .Build();

            // Resolve dependencies from the container
            var mainProcess = host.Services.GetRequiredService<IHostedService>();

            await host.RunAsync();
        }
    }
}
