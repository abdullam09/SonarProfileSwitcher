using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Models;

namespace SonarProfileSwitcher.Services
{
    public class MainProcess : BackgroundService
    {
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);
        private readonly ILogger<MainProcess> _logger;
        private readonly ISteelSeriesSonarService _steelSeriesSonarService;
        private readonly IProfileServices _profileServices;
        private readonly IProcessServices _processServices;
        private readonly IWidgetStateService _widgetStateService;
        private readonly IKeyboardLayoutService _keyboardLayoutService;
        private bool noProfileActive = true;
        private SonarGamingConfiguration activeConfig;
        private string keyboardLayout;
        private Profile activeProfile;

        public MainProcess(ISteelSeriesSonarService steelSeriesSonarService, IProcessServices processServices,
            IProfileServices profileServices, IWidgetStateService widgetStateService, IKeyboardLayoutService keyboardLayoutService, ILogger<MainProcess> logger)
        {
            _logger = logger;
            _steelSeriesSonarService = steelSeriesSonarService;
            _profileServices = profileServices;
            _processServices = processServices;
            _widgetStateService = widgetStateService;
            _keyboardLayoutService = keyboardLayoutService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
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
                        UpdateWidget();
                    }

                    await Task.Delay(_interval, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    await Task.Delay(_interval, cancellationToken);
                }
            }
        }

        private async Task ActivateProfile(IEnumerable<SonarGamingConfiguration> sonarGamingConfigurations, Profile profile, CancellationToken cancellationToken)
        {
            activeProfile = profile;
            var matchedSonarGamingConfiguration = sonarGamingConfigurations.FirstOrDefault(c => c.Name == profile.profileName);
            if (matchedSonarGamingConfiguration != null)
            {
                noProfileActive = false;
                if (activeConfig == null || activeConfig.Id != matchedSonarGamingConfiguration.Id)
                {
                    await _steelSeriesSonarService.ChangeSelectedGamingConfiguration(matchedSonarGamingConfiguration, cancellationToken);
                    activeProfile = profile;
                    UpdateWidget();
                    activeConfig = matchedSonarGamingConfiguration;
                    _logger.LogInformation($"Activate profile {profile.profileName}");
                }
            }
        }

        private void UpdateWidget()
        {
            _widgetStateService.Update(activeProfile?.profileName ?? "Flat", keyboardLayout ?? "");
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Main Process stopped");
            return base.StopAsync(cancellationToken);
        }
    }
}
