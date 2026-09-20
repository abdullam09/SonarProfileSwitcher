using System.Diagnostics;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Services;

namespace SonarProfileSwitcher.UI;

internal sealed class ProfileManagerForm : Form
{
    private readonly ProfileServices profiles;
    private readonly ISteelSeriesSonarService sonar;
    private readonly TextBox search = new() { Dock = DockStyle.Fill, PlaceholderText = "Filter by executable, window title, or PID" };
    private readonly ListView processes = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
    private readonly TextBox executable = new() { Dock = DockStyle.Fill, PlaceholderText = "Select a process, browse, or paste an exe name/path" };
    private readonly ComboBox configurations = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ListView mappings = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false };
    private readonly Label status = new() { AutoSize = true, MaximumSize = new Size(740, 0) };
    private List<(string Name, string Title, int Id)> snapshot = new();

    public ProfileManagerForm(ProfileServices profiles, ISteelSeriesSonarService sonar)
    {
        this.profiles = profiles;
        this.sonar = sonar;
        Text = "Sonar Profile Switcher — Link a game";
        Size = new Size(820, 720);
        MinimumSize = new Size(680, 600);
        StartPosition = FormStartPosition.CenterScreen;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 9 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        for (var i = 0; i < 9; i++)
            layout.RowStyles.Add(new RowStyle(i is 2 or 7 ? SizeType.Percent : SizeType.AutoSize, i == 2 ? 60 : i == 7 ? 40 : 0));
        Controls.Add(layout);
        layout.Controls.Add(new Label { Text = "Choose the game executable shown in Task Manager (Details tab).", AutoSize = true }, 0, 0);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 0)!, 2);
        layout.Controls.Add(search, 0, 1);
        layout.Controls.Add(Button("Refresh processes", RefreshProcesses), 1, 1);
        processes.Columns.Add("Executable", 210);
        processes.Columns.Add("PID", 75);
        processes.Columns.Add("Window title", 430);
        layout.Controls.Add(processes, 0, 2);
        layout.SetColumnSpan(processes, 2);
        layout.Controls.Add(executable, 0, 3);
        layout.Controls.Add(Button("Browse .exe…", BrowseExecutable), 1, 3);
        layout.Controls.Add(configurations, 0, 4);
        layout.Controls.Add(Button("Refresh Sonar profiles", RefreshConfigurations), 1, 4);
        layout.Controls.Add(Button("Save link", Save), 1, 5);
        layout.Controls.Add(new Label { Text = "Saved links — select one to edit", AutoSize = true }, 0, 6);
        mappings.Columns.Add("Executable", 250);
        mappings.Columns.Add("Sonar profile", 460);
        layout.Controls.Add(mappings, 0, 7);
        layout.SetColumnSpan(mappings, 2);
        layout.Controls.Add(status, 0, 8);
        layout.SetColumnSpan(status, 2);
        search.TextChanged += (_, _) => RenderProcesses();
        processes.SelectedIndexChanged += (_, _) =>
        {
            if (processes.SelectedItems.Count > 0)
                executable.Text = processes.SelectedItems[0].Text;
        };
        mappings.SelectedIndexChanged += (_, _) =>
        {
            if (mappings.SelectedItems.Count == 0) return;
            var item = mappings.SelectedItems[0];
            executable.Text = item.Text;
            configurations.SelectedItem = item.SubItems[1].Text;
        };
        Shown += (_, _) => { RefreshProcesses(); RefreshConfigurations(); RefreshMappings(); };
    }

    private static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(4) };
        button.Click += (_, _) => action();
        return button;
    }

    private void RefreshProcesses()
    {
        snapshot.Clear();
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try { snapshot.Add((process.ProcessName + ".exe", process.MainWindowTitle, process.Id)); }
                catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or NotSupportedException)
                { /* Processes can exit or deny access while being enumerated. */ }
            }
        }
        RenderProcesses();
    }

    private void RenderProcesses()
    {
        processes.BeginUpdate();
        processes.Items.Clear();
        foreach (var process in snapshot
                     .Where(p => $"{p.Name} {p.Title} {p.Id}".Contains(search.Text, StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(p => !string.IsNullOrEmpty(p.Title)).ThenBy(p => p.Name))
            processes.Items.Add(new ListViewItem(new[] { process.Name, process.Id.ToString(), process.Title }));
        processes.EndUpdate();
    }

    private void BrowseExecutable()
    {
        using var dialog = new OpenFileDialog { Filter = "Executable files (*.exe)|*.exe", CheckFileExists = true };
        if (dialog.ShowDialog(this) == DialogResult.OK)
            executable.Text = dialog.FileName;
    }

    private void RefreshConfigurations()
    {
        try
        {
            var selected = configurations.SelectedItem as string;
            var names = sonar.GetGamingConfigurations().Select(p => p.Name).Distinct().OrderBy(n => n).ToArray();
            configurations.Items.Clear();
            configurations.Items.AddRange(names);
            if (selected != null) configurations.SelectedItem = selected;
            if (names.Length == 0) status.Text = "No gaming profiles found. Open SteelSeries GG and enable Sonar, then refresh.";
            else status.Text = "Choose a Sonar gaming profile to link.";
        }
        catch (Exception ex) { ShowError("Could not load Sonar profiles. Open SteelSeries GG, then refresh.", ex); }
    }

    private void RefreshMappings()
    {
        try
        {
            var saved = profiles.GetProfiles();
            mappings.Items.Clear();
            foreach (var mapping in saved)
                mappings.Items.Add(new ListViewItem(new[] { mapping.exeFile, mapping.profileName }));
        }
        catch (Exception ex) { ShowError("Could not read saved links.", ex); }
    }

    private void Save()
    {
        if (configurations.SelectedItem is not string profile || string.IsNullOrWhiteSpace(executable.Text))
        {
            status.Text = "Choose an executable and a Sonar profile first.";
            return;
        }
        try
        {
            profiles.SaveMapping(executable.Text, profile);
            RefreshMappings();
            status.Text = $"Saved to {profiles.FilePath}. The switcher picks up changes automatically.";
        }
        catch (Exception ex) { ShowError("Could not save the link.", ex); }
    }

    private void ShowError(string message, Exception ex)
    {
        status.Text = message;
        MessageBox.Show(this, $"{message}\n\n{ex.Message}", "Sonar Profile Switcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
