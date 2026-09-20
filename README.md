# SonarProfileSwitcher

Automatically switches the Sonar gaming profile for a running game.

## Link a game

1. Start SonarProfileSwitcher and your game.
2. Double-click its notification-area icon (or right-click it and choose **Link game to Sonar profile…**).
3. Find the game in the process list. Executable names and PIDs match Task Manager's **Details** tab; search by name, PID, or window title. Use **Refresh processes** after starting a game.
4. Select a Sonar gaming profile and click **Save link**.

You can also browse to a game executable while it is stopped, or paste its executable name/path. The app saves the process name without the .exe extension, which is what the switcher matches. Games with the same executable name share a link regardless of installation folder.

Select an existing saved link to change its profile. Other links are preserved. Keep SteelSeries GG/Sonar installed and enabled so its gaming profiles can be loaded.

The app uses F:\Sonar Auto Switch\profiles.json when that file exists at startup; otherwise it uses profiles.json beside the application. The save status shows the full destination. Changes are picked up by the running switcher on its next polling cycle (about two seconds). Closing the manager leaves switching active; **Exit** in the tray menu stops it.

Launch with --manage to open the manager immediately:
    dotnet run --project SonarProfileSwitcher -- --manage

Requires Windows and the .NET 8 desktop runtime (or an SDK to build).
