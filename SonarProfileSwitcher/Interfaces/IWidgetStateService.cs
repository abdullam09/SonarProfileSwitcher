namespace SonarProfileSwitcher.Interfaces
{
    public interface IWidgetStateService : IDisposable
    {
        void Update(string profileName, string keyboardLayout);
    }
}
