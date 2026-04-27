# ClassicGrenades

Маленький плагин под **SCP: Secret Laboratory** на **LabAPI 1.1.6**.

Возвращает «классическое» поведение гранат: тиммейты не получают урон от своих,
но кидавший и враги — получают всё как обычно.

## Что делает

- HE-граната и SCP-018 не наносят урон между игроками одной фракции.
- Самому кидавшему — обычный урон от своей же гранаты (можно отключить).
- Врагам — ванильный урон, ничего не трогается.
- Любые не-взрывные источники урона — без изменений.

## Конфиг

`config.yml` (создаётся при первом запуске):

```yaml
On: true
BlockHe: true
Block018: true
SelfDmg: true
LogBlocks: false
```

- `On` — общий выключатель.
- `BlockHe` — блокировать урон от HE между тиммейтами.
- `Block018` — то же для SCP-018.
- `SelfDmg` — `true`: кидавший получает урон от своей гранаты; `false`: тоже неуязвим.
- `LogBlocks` — писать в консоль каждый заблокированный хит.

## Сборка

```bash
dotnet build ClassicGrenades/ClassicGrenades.csproj -c Release \
  -p:ScpslManagedDir=/path/to/scpsl_server/SCPSL_Data/Managed/
```

`ClassicGrenades.dll` положить в:

- Linux: `~/.config/SCP Secret Laboratory/LabAPI/plugins/global/`
- Windows: `%appdata%\SCP Secret Laboratory\LabAPI\plugins\global\`
