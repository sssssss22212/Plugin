using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace AdvancedServerManager.Modules
{
    /// <summary>
    /// Auto Team Balance module that monitors team sizes on respawn waves
    /// and balances teams when the difference exceeds the configured threshold.
    /// </summary>
    public class AutoBalanceModule
    {
        private readonly AutoBalanceConfig _config;

        public AutoBalanceModule(AutoBalanceConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            ServerEvents.RoundStarted += OnRoundStarted;
            PlayerEvents.Spawned += OnPlayerSpawned;

            LabApi.Features.Console.Logger.Info("[AutoBalance] Module enabled.");
        }

        public void Disable()
        {
            ServerEvents.RoundStarted -= OnRoundStarted;
            PlayerEvents.Spawned -= OnPlayerSpawned;

            LabApi.Features.Console.Logger.Info("[AutoBalance] Module disabled.");
        }

        private void OnRoundStarted()
        {
            LabApi.Features.Console.Logger.Info("[AutoBalance] Round started — monitoring team sizes.");
        }

        private void OnPlayerSpawned(PlayerSpawnedEventArgs args)
        {
            if (args.Player == null) return;

            // Check balance after each spawn event
            CheckAndBalance();
        }

        private void CheckAndBalance()
        {
            var players = Player.ReadyList.Where(p => p != null && p.IsAlive).ToList();

            int mtfCount = players.Count(p => p.Team == Team.FoundationForces);
            int chaosCount = players.Count(p => p.Team == Team.ChaosInsurgency);

            int diff = Math.Abs(mtfCount - chaosCount);

            if (diff <= _config.MaxTeamDifference) return;

            Team largerTeam = mtfCount > chaosCount ? Team.FoundationForces : Team.ChaosInsurgency;
            Team smallerTeam = largerTeam == Team.FoundationForces ? Team.ChaosInsurgency : Team.FoundationForces;
            int toMove = (diff - _config.MaxTeamDifference) / 2;

            if (toMove <= 0) return;

            // Get the most recently spawned players from the larger team
            var candidates = players
                .Where(p => p.Team == largerTeam)
                .Take(toMove)
                .ToList();

            foreach (var player in candidates)
            {
                RoleTypeId targetRole = smallerTeam == Team.FoundationForces
                    ? RoleTypeId.NtfPrivate
                    : RoleTypeId.ChaosConscript;

                player.SetRole(targetRole);

                player.SendBroadcast(
                    $"<color=cyan>[Auto-Balance]</color> You were moved to {smallerTeam} to maintain fair teams.",
                    6);
            }

            if (_config.BroadcastOnBalance)
            {
                string msg = $"<color=cyan>[Auto-Balance]</color> {toMove} player(s) moved to balance teams. ({mtfCount} MTF vs {chaosCount} CI)";
                foreach (var player in Player.ReadyList)
                {
                    player?.SendBroadcast(msg, 5);
                }
            }

            LabApi.Features.Console.Logger.Info($"[AutoBalance] Balanced teams: moved {toMove} from {largerTeam} to {smallerTeam}. (Was: {mtfCount} MTF vs {chaosCount} CI)");
        }

        /// <summary>
        /// Gets a summary of the current team sizes.
        /// </summary>
        public string GetTeamSummary()
        {
            var players = Player.ReadyList.Where(p => p != null && p.IsAlive).ToList();
            int mtf = players.Count(p => p.Team == Team.FoundationForces);
            int chaos = players.Count(p => p.Team == Team.ChaosInsurgency);
            int scp = players.Count(p => p.Team == Team.SCPs);
            int classD = players.Count(p => p.Role == RoleTypeId.ClassD);
            int scientist = players.Count(p => p.Role == RoleTypeId.Scientist);

            return $"MTF: {mtf} | CI: {chaos} | SCP: {scp} | Class-D: {classD} | Scientists: {scientist}";
        }
    }
}
