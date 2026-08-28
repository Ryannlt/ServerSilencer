# QuietAdmin

[![Latest release](https://img.shields.io/github/v/release/Ryannlt/QuietAdmin?label=latest&style=flat-square)](https://github.com/Ryannlt/QuietAdmin/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/Ryannlt/QuietAdmin/total?style=flat-square)](https://github.com/Ryannlt/QuietAdmin/releases)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE)

A [MelonLoader](https://github.com/LavaGang/MelonLoader) mod for **Holdfast: Nations At War** that hides the chat
lines the server posts on the admin channel. Slays, revives, teleports and weapon grants all produce one. A server
mod that respawns bots can generate hundreds in a session and bury the chat pane.

Nothing else is touched. Broadcasts, player chat and private messages all behave normally.

This is a **client-side** mod. It changes nothing for anyone else on the server, works on any server you join, and
nobody else needs it installed.

## Install

**1. Install MelonLoader.** Get the installer from
[MelonLoader releases](https://github.com/LavaGang/MelonLoader/releases), point it at `Holdfast NaW.exe`, and
choose **v0.6.6** or later. Run the game once so it creates its folders.

**2. Download QuietAdmin.** Grab `QuietAdmin.dll` from the
[latest release](https://github.com/Ryannlt/QuietAdmin/releases/latest).

**3. Drop it in.** Put it in the `Mods` folder next to `Holdfast NaW.exe`:

```
Steam\steamapps\common\Holdfast Nations At War\Mods\QuietAdmin.dll
```

**4. Start the game.** That is all. Settings are written to `UserData\MelonPreferences.cfg` on first run.

### Did it work?

Open `MelonLoader\Latest.log` and look for:

```
Melon Assembly loaded: '.\Mods\QuietAdmin.dll'
```

If it is not there, the mod was skipped. See [Troubleshooting](#troubleshooting).

## Settings

`UserData\MelonPreferences.cfg`, under `[QuietAdmin]`. Edits apply to the next message, no restart needed.

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
admin-channel echo. QuietAdmin removes the echo and keeps the message, so server mods that reply to your commands
still work.

## Troubleshooting

**Nothing in the log at all.** MelonLoader is not running. Check that `version.dll` sits next to
`Holdfast NaW.exe` and that a `MelonLoader` folder exists.

**Mod is skipped with a game-mismatch message.** The mod checks it is running against Holdfast. This can happen if
Anvil renames the game entry. Open an issue with the `Game Name` and `Game Developer` lines from the top of your
log.

**Mod loads but nothing changes.** Check `BlockAdminChat` in `MelonPreferences.cfg` has not been turned off.

**The game crashes on launch after adding the mod.** Remove the `.dll`, launch to confirm the game is fine, then
open an issue with `MelonLoader\Latest.log` attached.

## How it works

One Harmony prefix on `ClientChatHandler.AddChatEntry(int, string, TextChatEntryType, TextChatChannel, string)`,
which every chat route funnels through. A line is dropped when its entry type is `AdminAction`. The game produces
that type in exactly one place, when a message arrives on the `Admin` channel with `playerID == -1`, meaning no
sender and therefore the server itself.

Private message types are exempted before that test, unconditionally.

A second prefix on `AdminFancyUINotificationPanel.ShowAdminNotification` covers the popups, off by default.

## Building

Only needed if you want to change something. Otherwise use the prebuilt release above.

**Requires:** the game installed, MelonLoader installed, and a
[.NET SDK](https://dotnet.microsoft.com/download) for the compiler.

```
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

That compiles the mod and copies the `.dll` into your game's `Mods\` folder. Restart the game to load it. Add
`-NoDeploy` to build without copying, which is useful while the game is running and holding the file.

`release.ps1` builds the same way and stages the `.dll` in a `Release\` folder ready to upload.

Both scripts expect the default install path:

```
C:\Program Files (x86)\Steam\steamapps\common\Holdfast Nations At War
```

Edit `$GameDir` at the top of the script if yours differs.

The `.csproj` is for IDE support only. `build.ps1` is the real build. It drives `csc` directly, which avoids
needing a .NET 3.5 targeting pack installed.

### Why there is no CI build

Building needs `Assembly-CSharp.dll` and the MelonLoader assemblies from a real install. Those are Anvil's and
LavaGang's files, not mine to redistribute, so they cannot be committed here and a GitHub Actions runner has no way
to get them. Releases are built locally and uploaded by hand.

## Compatibility

Built against Holdfast on Unity 2022.3.62f2 with MelonLoader 0.6.6 (Mono). It patches named methods rather than
offsets, so it usually survives game updates. If Anvil renames or restructures the chat system it will stop working
rather than misbehave. Open an issue if that happens.

## Licence

[MIT](LICENSE). Do what you like with it, no warranty.
