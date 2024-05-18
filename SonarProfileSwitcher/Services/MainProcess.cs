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
        private readonly IKeyboardLayoutService _keyboardLayoutService;
        private bool noProfileActive = true;
        private SonarGamingConfiguration activeConfig;
        private string keyboardLayout;
        private Profile activeProfile;

        public MainProcess(ISteelSeriesSonarService steelSeriesSonarService, IProcessServices processServices,
            IProfileServices profileServices, ISmartScreenServices smartScreenServices, IKeyboardLayoutService keyboardLayoutService, ILogger<MainProcess> logger)
        {
            _logger = logger;
            _steelSeriesSonarService = steelSeriesSonarService;
            _profileServices = profileServices;
            _processServices = processServices;
            _smartScreenServices = smartScreenServices;
            _keyboardLayoutService = keyboardLayoutService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Main Process started");

            var defaultProfile = new Profile
            {
                profileName = "Flat",
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

                    var activeKeyboardLayout = await _keyboardLayoutService.CheckKeyboardLayout();
                    if (!activeKeyboardLayout.Equals(keyboardLayout))
                    {
                        keyboardLayout = activeKeyboardLayout;
                        _logger.LogInformation($"Activate keyboard Layout {keyboardLayout}");
                        await PrintToSmartScreen();
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
                    activeProfile = profile;
                    await PrintToSmartScreen();
                    activeConfig = matchedSonarGamingConfiguration;
                    _logger.LogInformation($"Activate profile {profile.profileName}");
                }
            }
        }

        private async Task PrintToSmartScreen()
        {
            await _smartScreenServices.Print(activeProfile.profileName, keyboardLayout);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Main Process stopped");
            return Task.CompletedTask;
        }
    }
}
