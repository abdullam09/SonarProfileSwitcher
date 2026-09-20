using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Services;

namespace SonarProfileSwitcher.UI;

internal sealed class ProfileTrayContext : ApplicationContext
{
    private readonly NotifyIcon tray;
    private readonly Icon applicationIcon;
    private readonly ContextMenuStrip menu = new();
    private readonly ProfileServices profiles;
    private readonly ISteelSeriesSonarService sonar;
    private ProfileManagerForm? manager;

    public ProfileTrayContext(ProfileServices profiles, ISteelSeriesSonarService sonar, bool showManager)
    {
        using var iconStream = typeof(ProfileTrayContext).Assembly.GetManifestResourceStream("SonarProfileSwitcher.Assets.SonarProfileSwitcher.ico")
            ?? throw new InvalidOperationException("Application icon resource is missing.");
        using var sourceIcon = new Icon(iconStream);
        applicationIcon = (Icon)sourceIcon.Clone();
        this.profiles = profiles;
        this.sonar = sonar;
        menu.Items.Add("Link game to Sonar profile…", null, (_, _) => ShowManager());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
        tray = new NotifyIcon
        {
            Icon = applicationIcon,
            Text = "Sonar Profile Switcher",
            ContextMenuStrip = menu,
            Visible = true
        };
        tray.DoubleClick += (_, _) => ShowManager();
        if (showManager) ShowManager();
    }

    private void ShowManager()
    {
        if (manager is null || manager.IsDisposed)
            manager = new ProfileManagerForm(profiles, sonar) { Icon = applicationIcon };
        manager.Show();
        if (manager.WindowState == FormWindowState.Minimized)
            manager.WindowState = FormWindowState.Normal;
        manager.Activate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            tray.Visible = false;
            tray.Dispose();
            menu.Dispose();
            manager?.Dispose();
            applicationIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
