using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Modules
{
    public class ScpInfoModule
    {
        private readonly ScpInfoConfig _config;

        public ScpInfoModule(ScpInfoConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            if (_config.ShowOnSpawn)
                PlayerEvents.Spawned += OnPlayerSpawned;
            Logger.Info("[ScpInfo] Module enabled.");
        }

        public void Disable()
        {
            PlayerEvents.Spawned -= OnPlayerSpawned;
            Logger.Info("[ScpInfo] Module disabled.");
        }

        private void OnPlayerSpawned(PlayerSpawnedEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsReady || ev.Player.IsHost)
                return;

            if (!ev.Player.IsSCP)
                return;

            string hint = BuildScpInfoHint(ev.Player);
            ev.Player.SendHint(hint, _config.HintDuration);
        }

        private string BuildScpInfoHint(Player player)
        {
            var lines = new List<string>();

            string scpName = HintBuilder.RoleName(player.Role);
            lines.Add(HintBuilder.Header($"You are {scpName}", "#ff0000", 26));
            lines.Add(HintBuilder.Separator("#660000"));

            switch (player.Role)
            {
                case RoleTypeId.Scp173:
                    lines.Add(HintBuilder.Body("Snap necks of anyone who looks away", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Place tantrum to block vision", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Use Breakneck Speed for fast movement", "#ffcccc"));
                    break;
                case RoleTypeId.Scp106:
                    lines.Add(HintBuilder.Body("Send players to the Pocket Dimension", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Use Hunter's Atlas to teleport", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Stalk mode lets you ambush targets", "#ffcccc"));
                    break;
                case RoleTypeId.Scp049:
                    lines.Add(HintBuilder.Body("Touch to kill, then resurrect as 049-2", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Use Doctor's Call to find targets", "#ffcccc"));
                    lines.Add(HintBuilder.Body("The Sense ability reveals nearby players", "#ffcccc"));
                    break;
                case RoleTypeId.Scp096:
                    lines.Add(HintBuilder.Body("Anyone who sees your face triggers rage", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Charge to close distance quickly", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Try Not To Cry to recover faster", "#ffcccc"));
                    break;
                case RoleTypeId.Scp939:
                    lines.Add(HintBuilder.Body("Detect players by sound visualization", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Create amnestic clouds to disorient", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Lunge to close gaps and attack", "#ffcccc"));
                    break;
                case RoleTypeId.Scp079:
                    lines.Add(HintBuilder.Body("Control cameras to observe the facility", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Lock doors, blackout rooms, use Tesla gates", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Gain XP from assisting other SCPs", "#ffcccc"));
                    break;
                case RoleTypeId.Scp3114:
                    lines.Add(HintBuilder.Body("Disguise as humans using ragdolls", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Strangle targets from behind", "#ffcccc"));
                    lines.Add(HintBuilder.Body("Dance to confuse your enemies", "#ffcccc"));
                    break;
                default:
                    lines.Add(HintBuilder.Body("Eliminate all humans to win!", "#ffcccc"));
                    break;
            }

            lines.Add(HintBuilder.Separator("#660000"));

            if (_config.ShowTargetCount)
            {
                int aliveHumans = 0;
                foreach (var p in Player.ReadyList)
                {
                    if (p != null && p.IsAlive && p.IsHuman)
                        aliveHumans++;
                }
                lines.Add(HintBuilder.Small($"Humans alive: {aliveHumans}", "#ff9999"));
            }

            int aliveScps = 0;
            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsAlive && p.IsSCP)
                    aliveScps++;
            }
            lines.Add(HintBuilder.Small($"SCPs alive: {aliveScps}", "#ff6666"));

            return string.Join("\n", lines);
        }
    }
}
