using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Services;
using SonarProfileSwitcher.UI;
using Serilog;
using Serilog.Formatting.Compact;

namespace SonarProfileSwitcher;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(new CompactJsonFormatter(), @"F:\Sonar Auto Switch\logs\log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        using var host = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.AddHostedService<MainProcess>();
                services.AddTransient<ISteelSeriesSonarService, SteelSeriesSonarService>();
                services.AddTransient<IFileServices, FileServices>();
                services.AddSingleton<ProfileServices>();
                services.AddSingleton<IProfileServices>(provider => provider.GetRequiredService<ProfileServices>());
                services.AddTransient<IProcessServices, ProcessServices>();
                services.AddTransient<IKeyboardLayoutService, KeyboardLayoutService>();
                services.AddSingleton<IWidgetStateService, WidgetStateService>();
            })
            .Build();
        host.StartAsync().GetAwaiter().GetResult();
        try
        {
            using var tray = new ProfileTrayContext(
                host.Services.GetRequiredService<ProfileServices>(),
                host.Services.GetRequiredService<ISteelSeriesSonarService>(),
                args.Contains("--manage", StringComparer.OrdinalIgnoreCase));
            Application.Run(tray);
        }
        finally
        {
            host.StopAsync().GetAwaiter().GetResult();
            Log.CloseAndFlush();
        }
    }
}
