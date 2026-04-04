using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace AdvancedServerManager.Modules
{
    /// <summary>
    /// Player Statistics module that tracks kills, deaths, escapes, and items per round.
    /// Shows a summary broadcast to each player at round end.
    /// </summary>
    public class StatsModule
    {
        private readonly StatsConfig _config;
        private readonly Dictionary<string, PlayerRoundStats> _roundStats = new Dictionary<string, PlayerRoundStats>();

        public StatsModule(StatsConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;

            if (_config.TrackKills)
            {
                PlayerEvents.Death += OnPlayerDeath;
            }

            if (_config.TrackItems)
            {
                PlayerEvents.PickedUpItem += OnPlayerPickedUpItem;
                PlayerEvents.UsedItem += OnPlayerUsedItem;
            }

            if (_config.TrackEscapes)
            {
                PlayerEvents.Escaped += OnPlayerEscaped;
            }

            PlayerEvents.Joined += OnPlayerJoined;

            LabApi.Features.Console.Logger.Info("[Stats] Module enabled.");
        }

        public void Disable()
        {
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;
            PlayerEvents.Death -= OnPlayerDeath;
            PlayerEvents.PickedUpItem -= OnPlayerPickedUpItem;
            PlayerEvents.UsedItem -= OnPlayerUsedItem;
            PlayerEvents.Escaped -= OnPlayerEscaped;
            PlayerEvents.Joined -= OnPlayerJoined;

            LabApi.Features.Console.Logger.Info("[Stats] Module disabled.");
        }

        private void OnRoundStarted()
        {
            _roundStats.Clear();

            foreach (var player in Player.ReadyList)
            {
                if (player == null) continue;
                EnsureStats(player);
            }
        }

        private void OnPlayerJoined(PlayerJoinedEventArgs args)
        {
            if (args.Player != null)
                EnsureStats(args.Player);
        }

        private void OnPlayerDeath(PlayerDeathEventArgs args)
        {
            if (args.Player != null)
            {
                var victimStats = EnsureStats(args.Player);
                victimStats.Deaths++;
            }

            if (args.Attacker != null && args.Attacker != args.Player)
            {
                var killerStats = EnsureStats(args.Attacker);
                killerStats.Kills++;
            }
        }

        private void OnPlayerPickedUpItem(PlayerPickedUpItemEventArgs args)
        {
            if (args.Player == null) return;
            var stats = EnsureStats(args.Player);
            stats.ItemsPickedUp++;
        }

        private void OnPlayerUsedItem(PlayerUsedItemEventArgs args)
        {
            if (args.Player == null) return;
            var stats = EnsureStats(args.Player);
            stats.ItemsUsed++;
        }

        private void OnPlayerEscaped(PlayerEscapedEventArgs args)
        {
            if (args.Player == null) return;
            var stats = EnsureStats(args.Player);
            stats.Escaped = true;
        }

        private void OnRoundEnded(RoundEndedEventArgs args)
        {
            if (!_config.ShowOnRoundEnd) return;

            foreach (var kvp in _roundStats)
            {
                var player = Player.ReadyList.FirstOrDefault(p => p?.UserId == kvp.Key);
                if (player == null) continue;

                var stats = kvp.Value;
                var sb = new StringBuilder();
                sb.AppendLine("<color=yellow><b>--- Your Round Stats ---</b></color>");

                if (_config.TrackKills)
                {
                    sb.AppendLine($"Kills: <color=green>{stats.Kills}</color> | Deaths: <color=red>{stats.Deaths}</color> | K/D: <color=cyan>{stats.KdRatio:F1}</color>");
                }

                if (_config.TrackItems)
                {
                    sb.AppendLine($"Items picked up: <color=cyan>{stats.ItemsPickedUp}</color> | Items used: <color=cyan>{stats.ItemsUsed}</color>");
                }

                if (_config.TrackEscapes && stats.Escaped)
                {
                    sb.AppendLine("<color=green><b>You escaped!</b></color>");
                }

                player.SendBroadcast(sb.ToString(), 10);
            }

            // Log top killer
            if (_roundStats.Count > 0)
            {
                var topKiller = _roundStats.OrderByDescending(x => x.Value.Kills).First();
                var topPlayer = Player.ReadyList.FirstOrDefault(p => p?.UserId == topKiller.Key);
                string topName = topPlayer?.Nickname ?? topKiller.Key;
                LabApi.Features.Console.Logger.Info($"[Stats] Round ended. Top killer: {topName} with {topKiller.Value.Kills} kills.");
            }
        }

        private PlayerRoundStats EnsureStats(Player player)
        {
            string odString = player.UserId;
            if (string.IsNullOrEmpty(odString))
                return new PlayerRoundStats();

            if (!_roundStats.TryGetValue(odString, out var stats))
            {
                stats = new PlayerRoundStats { Nickname = player.Nickname };
                _roundStats[odString] = stats;
            }
            return stats;
        }

        /// <summary>
        /// Gets stats for a specific player in the current round.
        /// </summary>
        public PlayerRoundStats GetStats(string odString)
        {
            _roundStats.TryGetValue(odString, out var stats);
            return stats;
        }

        /// <summary>
        /// Gets a leaderboard string for the current round.
        /// </summary>
        public string GetLeaderboard(int topN = 5)
        {
            var sorted = _roundStats
                .OrderByDescending(x => x.Value.Kills)
                .Take(topN)
                .ToList();

            if (sorted.Count == 0)
                return "No stats recorded yet.";

            var sb = new StringBuilder();
            sb.AppendLine("--- Leaderboard ---");
            int rank = 1;
            foreach (var kvp in sorted)
            {
                sb.AppendLine($"#{rank}: {kvp.Value.Nickname} — {kvp.Value.Kills}K / {kvp.Value.Deaths}D (K/D: {kvp.Value.KdRatio:F1})");
                rank++;
            }

            return sb.ToString();
        }
    }

    public class PlayerRoundStats
    {
        public string Nickname { get; set; } = "";
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int ItemsPickedUp { get; set; }
        public int ItemsUsed { get; set; }
        public bool Escaped { get; set; }

        public float KdRatio => Deaths == 0 ? Kills : (float)Kills / Deaths;
    }
}
