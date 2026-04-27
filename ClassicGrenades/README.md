# ClassicGrenades

Classic-style team-protected grenades for **SCP: Secret Laboratory**, built on
[LabAPI 1.1.6](https://github.com/northwood-studios/LabAPI/releases/tag/1.1.6).

## What it does

Restores the "classic" friendly-fire rule for explosions:

- **Teammates take 0 damage** from each other's HE grenades and SCP-018 balls.
- **The thrower still takes self-damage** from their own grenade (configurable).
- **Enemies always take full damage** — vanilla behaviour preserved.

Useful on classic / chill servers where friendly fire is on for guns but you
want to keep grenades safe to use without team-killing your squad.

## How it works

Subscribes to `PlayerEvents.Hurting`. When the damage source is
`ExplosionDamageHandler` (HE) or `Scp018DamageHandler` (SCP-018) and both the
attacker and victim are on the same `Faction` (and victim ≠ attacker), the event
is cancelled (`IsAllowed = false`).

All other damage sources are untouched.

## Config

`config.yml` (auto-generated on first run):

```yaml
IsEnabled: true
ProtectTeammatesFromHeGrenades: true
ProtectTeammatesFromScp018: true
ThrowerSelfDamage: true
LogBlockedHits: false
```

## Build

```bash
dotnet build ClassicGrenades/ClassicGrenades.csproj -c Release \
  -p:ScpslManagedDir=/path/to/scpsl_server/SCPSL_Data/Managed/
```

Drop `ClassicGrenades.dll` into:

- Linux: `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/`
- Windows: `%appdata%\SCP Secret Laboratory\LabAPI\plugins\global\`
