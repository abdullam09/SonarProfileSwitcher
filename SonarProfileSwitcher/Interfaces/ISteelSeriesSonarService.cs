using SonarProfileSwitcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarProfileSwitcher.Interfaces
{
    public interface ISteelSeriesSonarService
    {
        IEnumerable<SonarGamingConfiguration> GetGamingConfigurations();

        Task ChangeSelectedGamingConfiguration(SonarGamingConfiguration sonarGamingConfiguration,
            CancellationToken cancellationToken);
    }
}
