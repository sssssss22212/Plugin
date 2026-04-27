# UltimateServerToolkit

Hardcore RP toolkit for **SCP: Secret Laboratory** dedicated servers, built on
[**LabAPI 1.1.6**](https://github.com/northwood-studios/LabAPI/releases/tag/1.1.6).

A single, opinionated plugin that turns a vanilla SCP:SL server into an RP-friendly
environment with persistent character profiles, karma, anti-RDM, role whitelist,
in-world notes, GM tools, and a structured round log.

## Features

| Module        | What it does                                                                                  |
| ------------- | --------------------------------------------------------------------------------------------- |
| **Profiles**  | Persistent per-user RP profile: name, bio, faction, karma, RDM/team-kill counts. YAML-stored. |
| **DisplayName** | Replaces the player's nickname above their head with their RP name.                          |
| **Karma**     | Penalties for RDM / team-kills, bonuses for round-survival, optional auto-kick at 0 karma.     |
| **AntiRdm**   | Detects unprovoked same-team damage and auto-applies karma penalties + warning.               |
| **Whitelist** | UserId whitelist on configurable (usually SCP) roles.                                          |
| **Notes**     | Players can drop in-world notes that anyone within range can read.                            |
| **RoundLog**  | Append-only per-round log of joins, deaths, RP actions, GM events.                            |
| **GM Tools**  | RA / whitelisted-GM commands: `gm tp`, `gm bring`, `gm give`, `gm heal`, `gm god`, `gm freeze`, `gm broadcast`, `gm event`, `gm karma`. |

## Player commands (in-game `.` prefix)

| Command       | Usage                                  | Effect                                                  |
| ------------- | -------------------------------------- | ------------------------------------------------------- |
| `.me`         | `.me ducks behind the crate`           | Local RP action visible within ~8 m.                    |
| `.try`        | `.try kick the door open`              | Same as `.me` but with a randomized success/fail roll.  |
| `.do`         | `.do The lights flicker overhead`      | Narrate environment (no name shown).                    |
| `.look`       | `.look`                                | Read the nearest player's RP bio.                       |
| `.whisper`    | `.whisper meet me at LCZ checkpoint`   | Whisper visible only within ~3 m.                       |
| `.rpname`     | `.rpname Dr. Vasiliev`                 | Set your RP character name.                             |
| `.bio`        | `.bio Bald, mid-50s, smells of bleach` | Set your RP description.                                |
| `.rpfaction`  | `.rpfaction MTF E-11`                  | Tag your in-character faction.                          |
| `.karma`      | `.karma`                               | Show your karma + RP record.                            |
| `.note write` | `.note write Watch out for SCP-049`    | Drop a written note at your current position.           |
| `.note read`  | `.note read`                           | Read all notes within ~2.5 m.                           |

## Game-Master commands (Remote Admin or whitelisted UserIds)

```
gm tp <player>
gm bring <player>
gm give <player> <ItemType>
gm heal <player>
gm god <player>
gm freeze <player>
gm broadcast <message>
gm event <text>
gm karma <player> <±amount> [reason...]
```

## Config

Generated on first start at `~/.config/SCP Secret Laboratory/LabAPI/configs/<port>/UltimateServerToolkit/config.yml`.
See `Config.cs` for every tunable (range, format strings, karma thresholds, whitelist roles, etc.).

Persistent data (profiles + round logs) lives next to the DLL in
`UltimateServerToolkit-Data/`.

## Build

Requires the SCP:SL dedicated server's managed DLLs (LabAPI 1.1.6). Pass the path
via `ScpslManagedDir`:

```bash
dotnet build UltimateServerToolkit/UltimateServerToolkit.csproj -c Release \
  -p:ScpslManagedDir=/path/to/scpsl_server/SCPSL_Data/Managed/
```

The compiled `UltimateServerToolkit.dll` goes into:

- Linux: `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/`
- Windows: `%appdata%\SCP Secret Laboratory\LabAPI\plugins\global\`

## Compatibility

- LabAPI **1.1.6** (compiled against `LabApi.dll` shipped with SCP:SL game version 14.2.6).
- Target framework: `.NET Framework 4.8`.
- License: same as the parent repo.
