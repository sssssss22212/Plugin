using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Modules
{
    public class RespawnTimerModule
    {
        private readonly RespawnTimerConfig _config;
        private CoroutineHandle _timerCoroutine;
        private bool _roundInProgress;

        public RespawnTimerModule(RespawnTimerConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;
            Logger.Info("[RespawnTimer] Module enabled.");
        }

        public void Disable()
        {
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;
            Timing.KillCoroutines(_timerCoroutine);
            Logger.Info("[RespawnTimer] Module disabled.");
        }

        private void OnRoundStarted()
        {
            _roundInProgress = true;
            _timerCoroutine = Timing.RunCoroutine(SpectatorInfoLoop());
        }

        private void OnRoundEnded(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            _roundInProgress = false;
            Timing.KillCoroutines(_timerCoroutine);
        }

        private IEnumerator<float> SpectatorInfoLoop()
        {
            while (_roundInProgress)
            {
                yield return Timing.WaitForSeconds(1.5f);

                try
                {
                    int aliveScps = 0;
                    int aliveHumans = 0;
                    int aliveDClass = 0;
                    int aliveScientists = 0;
                    int aliveMtf = 0;
                    int aliveChaos = 0;

                    foreach (var p in Player.ReadyList)
                    {
                        if (p == null || !p.IsAlive) continue;

                        if (p.IsSCP) aliveScps++;
                        if (p.IsHuman) aliveHumans++;

                        switch (p.Role)
                        {
                            case RoleTypeId.ClassD:
                                aliveDClass++;
                                break;
                            case RoleTypeId.Scientist:
                                aliveScientists++;
                                break;
                            case RoleTypeId.NtfPrivate:
                            case RoleTypeId.NtfSergeant:
                            case RoleTypeId.NtfSpecialist:
                            case RoleTypeId.NtfCaptain:
                            case RoleTypeId.FacilityGuard:
                                aliveMtf++;
                                break;
                            case RoleTypeId.ChaosConscript:
                            case RoleTypeId.ChaosRifleman:
                            case RoleTypeId.ChaosRepressor:
                            case RoleTypeId.ChaosMarauder:
                                aliveChaos++;
                                break;
                        }
                    }

                    foreach (var p in Player.ReadyList)
                    {
                        if (p == null || !p.IsReady || p.IsHost || p.Role != RoleTypeId.Spectator)
                            continue;

                        var hint = BuildSpectatorHint(aliveScps, aliveHumans, aliveDClass, aliveScientists, aliveMtf, aliveChaos);
                        p.SendHint(hint, 2f);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[RespawnTimer] Error: {ex.Message}");
                }
            }
        }

        private string BuildSpectatorHint(int scps, int humans, int dclass, int scientists, int mtf, int chaos)
        {
            var lines = new List<string>();

            lines.Add(HintBuilder.Separator("#444444"));
            lines.Add(HintBuilder.Header("SPECTATOR VIEW", "#00ffcc", 22));
            lines.Add(HintBuilder.Separator("#444444"));
            lines.Add("");

            lines.Add(HintBuilder.Size(
                HintBuilder.StatLine("SCPs Alive", scps.ToString(), "#ff4444", "#ff6666") +
                HintBuilder.Color("  |  ", "#555555") +
                HintBuilder.StatLine("Humans Alive", humans.ToString(), "#4488ff", "#66aaff"),
                16));

            lines.Add("");
            lines.Add(HintBuilder.Size(
                HintBuilder.Color($"D-Class: ", "#ff8c00") + HintBuilder.Bold(HintBuilder.Color(dclass.ToString(), "#ff8c00")) +
                HintBuilder.Color(" | ", "#555555") +
                HintBuilder.Color($"Scientists: ", "#ffff00") + HintBuilder.Bold(HintBuilder.Color(scientists.ToString(), "#ffff00")) +
                HintBuilder.Color(" | ", "#555555") +
                HintBuilder.Color($"MTF: ", "#0096ff") + HintBuilder.Bold(HintBuilder.Color(mtf.ToString(), "#0096ff")) +
                HintBuilder.Color(" | ", "#555555") +
                HintBuilder.Color($"CI: ", "#008f1e") + HintBuilder.Bold(HintBuilder.Color(chaos.ToString(), "#008f1e")),
                14));

            lines.Add("");

            if (Warhead.IsDetonationInProgress)
            {
                float time = Warhead.DetonationTime;
                lines.Add(HintBuilder.Size(
                    HintBuilder.Bold(HintBuilder.Color($"☢ WARHEAD: {time:F0}s ☢", "#ff3333")),
                    20));
            }

            lines.Add(HintBuilder.Separator("#444444"));
            lines.Add(HintBuilder.Small("You will respawn with the next wave", "#888888"));

            return string.Join("\n", lines);
        }
    }
}
