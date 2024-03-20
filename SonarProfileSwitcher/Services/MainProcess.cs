using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SonarProfileSwitcher.Services
{
    public class MainProcess : IHostedService
    {
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);
        private readonly ILogger<MainProcess> _logger;
        private readonly ISteelSeriesSonarService _steelSeriesSonarService;
        private readonly IProfileServices _profileServices;
        private readonly IProcessServices _processServices;
        private readonly ISmartScreenServices _smartScreenServices;
        private bool noProfileActive = true;
        private SonarGamingConfiguration activeConfig;

        public MainProcess(ISteelSeriesSonarService steelSeriesSonarService, IProcessServices processServices,
            IProfileServices profileServices, ISmartScreenServices smartScreenServices, ILogger<MainProcess> logger)
        {
            _logger = logger;
            _steelSeriesSonarService = steelSeriesSonarService;
            _profileServices = profileServices;
            _processServices = processServices;
            _smartScreenServices = smartScreenServices;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Main Process started");

            var defaultProfile = new Profile
            {
                profileName = "Default",
                exeFile = ""
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var profiles = _profileServices.GetProfiles();
                    var sonarGamingConfigurations = _steelSeriesSonarService.GetGamingConfigurations();
                    noProfileActive = true;
                    foreach (var profile in profiles)
                    {
                        if (string.IsNullOrEmpty(profile.profileName) || string.IsNullOrEmpty(profile.exeFile))
                        {
                            continue;
                        }

                        if (_processServices.exeFileExists(profile.exeFile))
                        {
                            await ActivateProfile(sonarGamingConfigurations, profile, cancellationToken);
                            if (!noProfileActive)
                            {
                                break;
                            }
                        }
                    }

                    if (noProfileActive)
                    {
                        await ActivateProfile(sonarGamingConfigurations, defaultProfile, cancellationToken);
                    }

                    await Task.Delay(_interval, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }

        private async Task ActivateProfile(IEnumerable<SonarGamingConfiguration> sonarGamingConfigurations, Profile profile, CancellationToken cancellationToken)
        {
            var matchedSonarGamingConfiguration = sonarGamingConfigurations.FirstOrDefault(c => c.Name == profile.profileName);
            if (matchedSonarGamingConfiguration != null)
            {
                noProfileActive = false;
                if (activeConfig == null || activeConfig.Id != matchedSonarGamingConfiguration.Id)
                {
                    await _steelSeriesSonarService.ChangeSelectedGamingConfiguration(matchedSonarGamingConfiguration, cancellationToken);
                    await _smartScreenServices.Print(profile.profileName);
                    activeConfig = matchedSonarGamingConfiguration;
                    _logger.LogInformation($"Activate profile {profile.profileName}");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Main Process stopped");
            return Task.CompletedTask;
        }
    }
}
