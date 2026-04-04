using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Modules
{
    public class CassieAutoModule
    {
        private readonly CassieAutoConfig _config;
        private int _generatorsActivated;

        public CassieAutoModule(CassieAutoConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            if (_config.AnnounceScpKills)
                PlayerEvents.Death += OnPlayerDeath;
            if (_config.AnnounceGenerators)
                ServerEvents.GeneratorActivated += OnGeneratorActivated;
            if (_config.AnnounceEscapes)
                PlayerEvents.Escaped += OnPlayerEscaped;
            ServerEvents.RoundStarted += OnRoundStarted;
            Logger.Info("[CassieAuto] Module enabled.");
        }

        public void Disable()
        {
            PlayerEvents.Death -= OnPlayerDeath;
            ServerEvents.GeneratorActivated -= OnGeneratorActivated;
            PlayerEvents.Escaped -= OnPlayerEscaped;
            ServerEvents.RoundStarted -= OnRoundStarted;
            Logger.Info("[CassieAuto] Module disabled.");
        }

        private void OnRoundStarted()
        {
            _generatorsActivated = 0;
        }

        private void OnPlayerDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsSCP)
                return;

            string scpNumber = GetScpNumber(ev.Player.Role);
            if (scpNumber == null)
                return;

            string killerInfo = "";
            if (ev.Attacker != null && ev.Attacker.IsHuman)
            {
                string team = ev.Attacker.IsNTF ? "NTF" : ev.Attacker.IsChaos ? "Chaos Insurgency" : "unknown forces";
                killerInfo = $" terminated by {team}";
            }

            // Count remaining SCPs
            int remainingScps = 0;
            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsAlive && p.IsSCP && p.UserId != ev.Player.UserId)
                    remainingScps++;
            }

            // Beautiful hint to all players
            string hint = HintBuilder.Size(
                HintBuilder.Bold(HintBuilder.Color($"SCP-{scpNumber} TERMINATED", "#ff0000")),
                28) + "\n" +
                HintBuilder.Size(
                    HintBuilder.Color($"Remaining SCPs: {remainingScps}", remainingScps > 0 ? "#ffcc00" : "#00ff00"),
                    18);

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(hint, 5f);
            }
        }

        private void OnGeneratorActivated(GeneratorActivatedEventArgs ev)
        {
            _generatorsActivated++;

            string hint = HintBuilder.Size(
                HintBuilder.Bold(HintBuilder.Color($"GENERATOR ACTIVATED ({_generatorsActivated}/3)", "#ffcc00")),
                24) + "\n" +
                HintBuilder.Size(
                    HintBuilder.ProgressBar(_generatorsActivated, 3, 15, "#ffcc00", "#333333"),
                    16);

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(hint, 4f);
            }

            Logger.Info($"[CassieAuto] Generator {_generatorsActivated}/3 activated.");
        }

        private void OnPlayerEscaped(PlayerEscapedEventArgs ev)
        {
            if (ev.Player == null)
                return;

            string roleName = HintBuilder.RoleName(ev.OldRole);
            string escapeMsg = HintBuilder.Size(
                HintBuilder.Bold(HintBuilder.Color("ESCAPE!", "#00ff00")),
                26) + "\n" +
                HintBuilder.Size(
                    roleName + HintBuilder.Color($" {ev.Player.Nickname} has escaped!", "#cccccc"),
                    18);

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(escapeMsg, 4f);
            }

            Logger.Info($"[CassieAuto] {ev.Player.Nickname} escaped as {ev.OldRole}.");
        }

        private string GetScpNumber(RoleTypeId role)
        {
            switch (role)
            {
                case RoleTypeId.Scp173: return "173";
                case RoleTypeId.Scp106: return "106";
                case RoleTypeId.Scp049: return "049";
                case RoleTypeId.Scp096: return "096";
                case RoleTypeId.Scp939: return "939";
                case RoleTypeId.Scp079: return "079";
                case RoleTypeId.Scp0492: return "049-2";
                case RoleTypeId.Scp3114: return "3114";
                default: return null;
            }
        }
    }
}
