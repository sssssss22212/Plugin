using System;
using System.Collections.Generic;
using System.Text;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Modules
{
    public class HudModule
    {
        private readonly HudConfig _config;
        private CoroutineHandle _hudCoroutine;
        private DateTime _roundStartTime;
        private bool _roundInProgress;

        public HudModule(HudConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            Logger.Info("[HUD] Module enabled.");
        }

        public void Disable()
        {
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            Timing.KillCoroutines(_hudCoroutine);
            Logger.Info("[HUD] Module disabled.");
        }

        private void OnWaitingForPlayers()
        {
            _roundInProgress = false;
        }

        private void OnRoundStarted()
        {
            _roundStartTime = DateTime.UtcNow;
            _roundInProgress = true;
            _hudCoroutine = Timing.RunCoroutine(HudUpdateLoop());
        }

        private void OnRoundEnded(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            _roundInProgress = false;
            Timing.KillCoroutines(_hudCoroutine);
        }

        private IEnumerator<float> HudUpdateLoop()
        {
            while (_roundInProgress)
            {
                yield return Timing.WaitForSeconds(_config.UpdateInterval);

                try
                {
                    foreach (var player in Player.ReadyList)
                    {
                        if (player == null || !player.IsReady || player.IsHost)
                            continue;

                        string hud = BuildHud(player);
                        player.SendHint(hud, _config.UpdateInterval + 0.5f);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[HUD] Error: {ex.Message}");
                }
            }
        }

        private string BuildHud(Player player)
        {
            var sb = new StringBuilder();

            // Top section - Server info (small, top-right feel via newlines)
            sb.AppendLine();

            if (_config.ShowServerName)
            {
                sb.AppendLine(HintBuilder.Size(HintBuilder.Bold(HintBuilder.Color(_config.ServerDisplayName, _config.HudColor)), 14));
            }

            // Stats line
            var statsLine = new List<string>();

            if (_config.ShowTps)
            {
                double tps = Server.Tps;
                string tpsColor = tps > 55 ? "#00ff00" : tps > 40 ? "#ffcc00" : "#ff3333";
                statsLine.Add(HintBuilder.Color($"TPS: {tps:F0}", tpsColor));
            }

            if (_config.ShowPlayerCount)
            {
                statsLine.Add(HintBuilder.Color($"Players: {Server.PlayerCount}/{Server.MaxPlayers}", "#aaaaaa"));
            }

            if (_config.ShowRoundTimer)
            {
                TimeSpan elapsed = DateTime.UtcNow - _roundStartTime;
                statsLine.Add(HintBuilder.Color($"Time: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}", "#aaaaaa"));
            }

            if (statsLine.Count > 0)
            {
                sb.AppendLine(HintBuilder.Size(string.Join(HintBuilder.Color(" | ", "#555555"), statsLine), 12));
            }

            // Player info section
            if (player.Role != RoleTypeId.Spectator && player.IsAlive)
            {
                sb.AppendLine();

                if (_config.ShowRole)
                {
                    sb.AppendLine(HintBuilder.Size(HintBuilder.RoleName(player.Role), 16));
                }

                if (_config.ShowHealth)
                {
                    string healthText = $" {player.Health:F0}/{player.MaxHealth:F0} HP";
                    sb.AppendLine(HintBuilder.Size(HintBuilder.HealthBar(player.Health, player.MaxHealth) + HintBuilder.Color(healthText, "#cccccc"), 14));

                    if (player.ArtificialHealth > 0)
                    {
                        string ahpText = $" +{player.ArtificialHealth:F0} AHP";
                        sb.AppendLine(HintBuilder.Size(HintBuilder.ProgressBar(player.ArtificialHealth, player.MaxArtificialHealth, 10, "#00aaff", "#333333") + HintBuilder.Color(ahpText, "#00aaff"), 12));
                    }

                    if (player.HumeShield > 0)
                    {
                        string hsText = $" {player.HumeShield:F0} HS";
                        sb.AppendLine(HintBuilder.Size(HintBuilder.ProgressBar(player.HumeShield, player.MaxHumeShield, 10, "#cc00ff", "#333333") + HintBuilder.Color(hsText, "#cc00ff"), 12));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
