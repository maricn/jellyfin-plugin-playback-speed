# Jellyfin Playback Speed

Playback Speed is a Jellyfin server plugin that adds a small playback speed control to Jellyfin Web. It is aimed at podcast, audiobook, lecture, and music-library audio playback where the web UI may not expose a speed selector.

## Status

This is an early plugin scaffold. It builds a Jellyfin plugin DLL and injects a client script into Jellyfin Web's `index.html` on server startup.

## How It Works

Jellyfin server plugins cannot directly change every native Jellyfin client. This plugin targets Jellyfin Web by inserting a script tag into Jellyfin Web's `index.html`. The script is served by the plugin at `/PlaybackSpeed/client.js` and sets `playbackRate` on the active `audio` or `video` element.

Because this modifies Jellyfin Web files, Docker or package installs may need file permissions that allow the Jellyfin server process to update the web `index.html`.

## Build

```sh
dotnet restore
dotnet publish Jellyfin.Plugin.PlaybackSpeed/Jellyfin.Plugin.PlaybackSpeed.csproj -c Release
```

The plugin DLL will be under `Jellyfin.Plugin.PlaybackSpeed/bin/Release/net9.0/publish/`.

## Manual Install

1. Build the plugin.
2. Copy `Jellyfin.Plugin.PlaybackSpeed.dll` into a folder under your Jellyfin plugins directory.
3. Restart Jellyfin.
4. Open Dashboard, Plugins, Playback Speed to adjust defaults.

Common plugin directories are documented by Jellyfin:

- Linux packages: `/var/lib/jellyfin/plugins/`
- Windows direct install: `%UserProfile%\AppData\Local\jellyfin\plugins`
- Windows tray install: `%ProgramData%\Jellyfin\Server\plugins`

## Repository Install

After the first release is published, add this repository URL in Jellyfin under Dashboard, Plugins, Repositories:

```text
https://raw.githubusercontent.com/stephenhoos/jellyfin-plugin-playback-speed/main/manifest.json
```

## Plugin Repository Release

`manifest.json` is included for the public plugin repository. Each release should publish a ZIP named `Jellyfin.Plugin.PlaybackSpeed_<version>.zip` and update the manifest checksum.

## Limitations

- This affects Jellyfin Web only.
- Native clients such as Roku, Android TV, iOS, Android, and Jellyfin Media Player need their own client support.
- Audio passthrough or client decoding limitations can prevent playback-rate changes from working on some devices.
