# ServerSilencer

[![Latest release](https://img.shields.io/github/v/release/Ryannlt/ServerSilencer?label=latest&style=flat-square)](https://github.com/Ryannlt/ServerSilencer/releases/latest)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/Ryannlt/ServerSilencer/blob/main/LICENSE)

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for **Holdfast: Nations At War** that hides the chat lines
the server posts on the admin channel. Slays, revives, teleports and weapon grants all produce one. A server mod
that respawns bots can generate hundreds in a session and bury the chat pane.

Nothing else is touched. Broadcasts, player chat and private messages all behave normally.

This is a **client-side** mod. It changes nothing for anyone else on the server, works on any server you join,
and nobody else needs it installed.

## Install

**With a mod manager.** Install through [r2modman](https://r2modman.com/) or Thunderstore Mod Manager and launch
the game from the manager. BepInEx is pulled in as a dependency, so there is nothing else to set up.

**By hand.** Install
[BepInExPack](https://thunderstore.io/c/holdfast-nations-at-war/p/BepInEx/BepInExPack/) into the game folder and
run the game once so it creates its folders. Then download `ServerSilencer.dll` from the
[latest release](https://github.com/Ryannlt/ServerSilencer/releases/latest) and put it here:

```
Steam\steamapps\common\Holdfast Nations At War\BepInEx\plugins\ServerSilencer\ServerSilencer.dll
```

### Did it work?

Open `BepInEx\LogOutput.log` and look for:

```
[Info   :   BepInEx] Loading [ServerSilencer 1.0.0]
```

If it is not there, the mod was not loaded. See [Troubleshooting](#troubleshooting).

## Settings

`BepInEx\config\com.ryannlt.serversilencer.cfg`, written on first run. Edits apply within a second, no restart
needed.

For an in-game editor instead of a text file, [ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager)
works, but only after setting `HideManagerGameObject = true` under `[Chainloader]` in
`BepInEx\config\BepInEx.cfg`. Holdfast's BepInEx pack ships that as `false`, which stops
ConfigurationManager drawing at all.

| Setting | Default | Effect |
| --- | --- | --- |
| `BlockAdminChat` | `true` | Hide the server's admin-channel chat lines. |
| `BlockNotifications` | `false` | Also hide the centre-screen admin notification popups. |

## What it hides

| | Hidden |
| --- | --- |
| Server admin-channel lines (slay, revive, teleport, give weapon) | **yes** |
| Private messages from the server or an admin | no |
| Server and admin broadcasts | no |
| Player chat, squad, orders | no |
| Centre-screen popups | only with `BlockNotifications` |

When a server mod sends you a private message, Holdfast shows it **twice**, once as the message and once as an
admin-channel echo. ServerSilencer removes the echo and keeps the message, so server mods that reply to your
commands still work.

## Troubleshooting

**Nothing in the log at all, or no log.** BepInEx is not running. Check that `winhttp.dll` and a `BepInEx`
folder sit next to `Holdfast NaW.exe`. If you use a mod manager, launch the game from it rather than from Steam.

**Mod loads but nothing changes.** Check `BlockAdminChat` in the config file has not been turned off. If the
log shows the mod loading but chat lines still appear, open an issue with the log attached.

**The game crashes on launch after adding the mod.** Remove the `.dll`, launch to confirm the game is fine, then
open an issue with `BepInEx\LogOutput.log` attached.

## Building

Only needed if you want to change something. Otherwise use the release above.

**Requires:** the game installed, BepInEx installed in an r2modman profile, and a
[.NET SDK](https://dotnet.microsoft.com/download) for the compiler.

```
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

That compiles the mod and copies the `.dll` into the r2modman profile's `BepInEx\plugins\ServerSilencer\`. Restart
the game to load it. Add `-NoDeploy` to build without copying, which is useful while the game is running and
holding the file. `-ProfileName` picks a profile other than `Dev`.

`package.ps1` builds the same way and stages an uploadable Thunderstore zip in `Package\`.

The script expects the default install path:

```
C:\Program Files (x86)\Steam\steamapps\common\Holdfast Nations At War
```

Edit `$GameDir` at the top of the script if yours differs.

The `.csproj` is for IDE support only. `build.ps1` is the real build. It drives `csc` directly, which avoids
needing a targeting pack installed.

### Why there is no CI build

Building needs `Assembly-CSharp.dll` from a real install. Those are AGS files, not mine to redistribute, so
they cannot be committed here and a GitHub Actions runner has no way to get them. Releases are built locally and
uploaded by hand.

## Compatibility

Built against Holdfast on Unity 2022.3.62f2 with BepInEx 5.4.23.5 (Mono). It patches named methods rather than
offsets, so it usually survives game updates. If AGS renames or restructures the chat system it will stop
working rather than misbehave. Open an issue if that happens.

## Licence

[MIT](https://github.com/Ryannlt/ServerSilencer/blob/main/LICENSE). Do what you like with it, no warranty.
