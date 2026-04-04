using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Modules
{
    public class KillstreakModule
    {
        private readonly KillstreakConfig _config;
        private readonly Dictionary<string, PlayerKillData> _killData = new Dictionary<string, PlayerKillData>();

        public KillstreakModule(KillstreakConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            PlayerEvents.Death += OnPlayerDeath;
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;
            Logger.Info("[Killstreaks] Module enabled.");
        }

        public void Disable()
        {
            PlayerEvents.Death -= OnPlayerDeath;
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;
            Logger.Info("[Killstreaks] Module disabled.");
        }

        private void OnRoundStarted()
        {
            _killData.Clear();
        }

        private void OnRoundEnded(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            ShowTopKillersHint();
        }

        private void OnPlayerDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null || ev.Attacker.UserId == ev.Player.UserId)
                return;

            string attackerId = ev.Attacker.UserId;

            if (!_killData.TryGetValue(attackerId, out PlayerKillData data))
            {
                data = new PlayerKillData();
                _killData[attackerId] = data;
            }

            data.TotalKills++;
            data.TotalDeaths = 0; // Reset deaths on kill for streak

            DateTime now = DateTime.UtcNow;
            if ((now - data.LastKillTime).TotalSeconds <= _config.KillstreakTimeout)
            {
                data.CurrentStreak++;
            }
            else
            {
                data.CurrentStreak = 1;
            }
            data.LastKillTime = now;

            if (data.CurrentStreak > data.BestStreak)
                data.BestStreak = data.CurrentStreak;

            // Grant health bonus
            if (_config.GrantHealthBonus && ev.Attacker.IsAlive)
            {
                float bonus = _config.HealthBonusPerKill;
                float newHealth = ev.Attacker.Health + bonus;
                if (newHealth > ev.Attacker.MaxHealth)
                    newHealth = ev.Attacker.MaxHealth;
                ev.Attacker.Health = newHealth;

                ev.Attacker.SendHint(
                    HintBuilder.Size(HintBuilder.Color($"+{bonus:F0} HP", "#00ff00"), 16),
                    1.5f);
            }

            // Announce killstreaks
            CheckKillstreak(ev.Attacker, data.CurrentStreak);

            // Show death hint to victim
            if (ev.Player != null && ev.Player.IsReady)
            {
                string deathHint = HintBuilder.Size(
                    HintBuilder.Color("Killed by ", "#ff4444") +
                    HintBuilder.Bold(HintBuilder.Color(ev.Attacker.Nickname, "#ffffff")),
                    20);
                ev.Player.SendHint(deathHint, 3f);
            }

            // Track victim deaths
            if (ev.Player != null)
            {
                string victimId = ev.Player.UserId;
                if (!_killData.TryGetValue(victimId, out PlayerKillData victimData))
                {
                    victimData = new PlayerKillData();
                    _killData[victimId] = victimData;
                }
                victimData.TotalDeaths++;
                victimData.CurrentStreak = 0;
            }
        }

        private void CheckKillstreak(Player player, int streak)
        {
            string title = null;
            string color = null;

            if (streak >= _config.GodlikeThreshold)
            {
                title = "GODLIKE";
                color = "#ff0000";
            }
            else if (streak >= _config.UltraKillThreshold)
            {
                title = "ULTRA KILL";
                color = "#ff6600";
            }
            else if (streak >= _config.TripleKillThreshold)
            {
                title = "TRIPLE KILL";
                color = "#ffcc00";
            }
            else if (streak >= _config.DoubleKillThreshold)
            {
                title = "DOUBLE KILL";
                color = "#00ffcc";
            }

            if (title == null)
                return;

            string banner = HintBuilder.KillstreakBanner(title, $"{player.Nickname} - {streak} kills!", color);

            if (_config.AnnounceToAll)
            {
                foreach (var p in Player.ReadyList)
                {
                    if (p != null && p.IsReady && !p.IsHost)
                        p.SendHint(banner, 3f);
                }
            }
            else
            {
                player.SendHint(banner, 3f);
            }

            Logger.Info($"[Killstreaks] {player.Nickname} achieved {title} ({streak} kills)!");
        }

        private void ShowTopKillersHint()
        {
            var topKillers = new List<KeyValuePair<string, PlayerKillData>>();
            foreach (var kvp in _killData)
            {
                if (kvp.Value.TotalKills > 0)
                    topKillers.Add(kvp);
            }
            topKillers.Sort((a, b) => b.Value.TotalKills.CompareTo(a.Value.TotalKills));

            if (topKillers.Count == 0)
                return;

            var lines = new List<string>();
            int rank = 1;
            foreach (var kvp in topKillers)
            {
                if (rank > 5) break;

                string playerName = "Unknown";
                foreach (var p in Player.ReadyList)
                {
                    if (p != null && p.UserId == kvp.Key)
                    {
                        playerName = p.Nickname;
                        break;
                    }
                }

                string medal = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : $"#{rank}";
                lines.Add($"{medal} {playerName} - {kvp.Value.TotalKills} kills (Best: {kvp.Value.BestStreak}x)");
                rank++;
            }

            string panel = HintBuilder.InfoPanel("TOP KILLERS", lines.ToArray(), "#ff6600");

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(panel, 8f);
            }
        }

        public PlayerKillData GetPlayerData(string odlayerId)
        {
            return _killData.TryGetValue(odlayerId, out PlayerKillData data) ? data : null;
        }

        public Dictionary<string, PlayerKillData> GetAllData()
        {
            return _killData;
        }
    }

    public class PlayerKillData
    {
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public DateTime LastKillTime { get; set; } = DateTime.MinValue;
    }
}
