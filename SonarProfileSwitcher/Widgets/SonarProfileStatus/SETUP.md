# Sonar Profile Status 1.1.0

Private-use iCUE widget for Xeneon Edge (dashboard_lcd), with responsive horizontal and vertical layouts.

## Install
1. Keep SonarProfileSwitcher running with SteelSeries GG/Sonar enabled.
2. Import SonarProfileStatus-1.1.0.icuewidget through iCUE's widget import UI and add it to your Xeneon Edge layout. If an older version is already added, replace that widget instance if iCUE does not update it automatically.
3. Open the widget settings to customize it. On Xeneon Edge, enable Custom Style to override the device's colors.

Requires Windows, iCUE 5.47 or later, and widget framework 1.4.0 or later. No extra plugins, accounts or API keys.

## Settings
- Show Keyboard Layout: on.
- Refresh Interval: 1 second; adjustable from 1 to 30 seconds.
- Text Color: #f5f8fc.
- Accent Color: #56e0d0 (headset and connection indicator).
- Background Color: #101c2c.
- Background Transparency: 0%; adjustable to 100%. Text and icons remain opaque.

## Connection and states
The widget only reads http://127.0.0.1:9783/status on this computer. No data is sent to external services.
The app's own polling cycle is about two seconds; a one-second widget refresh does not increase the app's sampling rate.

Loading shows Connecting. An empty profile shows No active profile. A connection failure preserves the last valid reading in memory with an Offline badge and its age. Invalid responses show Unavailable. Restarting/reloading the widget clears that in-memory reading.
Requests time out after three seconds, do not overlap, and pause while the widget is hidden.
A successful response restores the Connected state automatically.

The profile name comes from the existing app status endpoint; Connected indicates that endpoint is reachable, not an independent check that Sonar applied the profile. Very long profile names are ellipsized to keep the layout stable (the full text is available as a tooltip in browser preview).

## Validation
Validate with: icuewidget validate SonarProfileStatus
Package a clean folder containing index.html, manifest.json, translation.json and resources using: icuewidget package SonarProfileStatus
Do not include older .icuewidget archives in the source folder used for packaging.

Layout and connection states were checked in Chromium at Xeneon Edge S/M/L/XL horizontal and vertical dimensions. Final display and settings behavior should also be checked in iCUE on the physical device.
