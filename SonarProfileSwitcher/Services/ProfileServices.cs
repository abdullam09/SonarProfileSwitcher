using Newtonsoft.Json;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Models;

namespace SonarProfileSwitcher.Services;

public class ProfileServices : IProfileServices
{
    public string FilePath { get; } = File.Exists(@"F:\Sonar Auto Switch\profiles.json")
        ? @"F:\Sonar Auto Switch\profiles.json"
        : Path.Combine(AppContext.BaseDirectory, "profiles.json");

    public ProfileServices(IFileServices fileServices) { }

    public IList<Profile> GetProfiles()
    {
        if (!File.Exists(FilePath))
            return new List<Profile>();
        return JsonConvert.DeserializeObject<List<Profile>>(File.ReadAllText(FilePath))
            ?? throw new InvalidDataException("profiles.json must contain an array of profile mappings.");
    }

    public void SaveMapping(string executable, string profileName)
    {
        var name = Path.GetFileName(executable.Trim());
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(profileName))
            throw new ArgumentException("Choose an executable and a Sonar profile.");

        // Reload before editing to preserve mappings added since the manager was opened.
        var profiles = GetProfiles();
        var existing = profiles.FirstOrDefault(p =>
            string.Equals(p.exeFile, name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
            profiles.Add(new Profile { exeFile = name, profileName = profileName });
        else
            existing.profileName = profileName;

        // Readers see either the old document or the complete new document.
        var temporaryPath = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(profiles, Formatting.Indented));
            File.Move(temporaryPath, FilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
