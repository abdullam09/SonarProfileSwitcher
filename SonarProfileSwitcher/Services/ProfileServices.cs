using Newtonsoft.Json;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarProfileSwitcher.Services
{
    public class ProfileServices : IProfileServices
    {
        private readonly IFileServices _fileServices;
        public ProfileServices(IFileServices fileServices)
        {
            _fileServices = fileServices;
        }

        public IList<Profile> GetProfiles()
        {
            var fileContent = _fileServices.ReadFile(@"F:\Sonar Auto Switch\profiles.json");
            if (fileContent != null)
            {
                return JsonConvert.DeserializeObject<List<Profile>>(fileContent);
            }
            throw new InvalidOperationException("profiles contenct can't be parsed");
        }
    }
}
