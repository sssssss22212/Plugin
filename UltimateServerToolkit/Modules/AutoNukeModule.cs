using System;
using System.Collections.Generic;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Modules
{
    public class AutoNukeModule
    {
        private readonly AutoNukeConfig _config;
        private CoroutineHandle _nukeCoroutine;
        private bool _roundInProgress;
        private bool _nukeStarted;
        private DateTime _roundStartTime;

        public AutoNukeModule(AutoNukeConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;
            Logger.Info($"[AutoNuke] Module enabled. Auto-nuke at {_config.AutoNukeTimeMinutes} min, warning at {_config.WarningTimeMinutes} min.");
        }

        public void Disable()
        {
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;
            Timing.KillCoroutines(_nukeCoroutine);
            Logger.Info("[AutoNuke] Module disabled.");
        }

        private void OnRoundStarted()
        {
            _roundInProgress = true;
            _nukeStarted = false;
            _roundStartTime = DateTime.UtcNow;
            _nukeCoroutine = Timing.RunCoroutine(AutoNukeLoop());
        }

        private void OnRoundEnded(LabApi.Events.Arguments.ServerEvents.RoundEndedEventArgs ev)
        {
            _roundInProgress = false;
            Timing.KillCoroutines(_nukeCoroutine);
        }

        private IEnumerator<float> AutoNukeLoop()
        {
            float warningSeconds = _config.WarningTimeMinutes * 60f;
            float nukeSeconds = _config.AutoNukeTimeMinutes * 60f;

            bool warningSent = false;

            while (_roundInProgress)
            {
                yield return Timing.WaitForSeconds(5f);

                float elapsed = (float)(DateTime.UtcNow - _roundStartTime).TotalSeconds;

                // Warning
                if (!warningSent && elapsed >= warningSeconds && _config.AnnounceWarning)
                {
                    warningSent = true;
                    float minutesLeft = (_config.AutoNukeTimeMinutes - _config.WarningTimeMinutes);
                    string warning = HintBuilder.WarningBanner(
                        "AUTO-NUKE WARNING",
                        $"Warhead will detonate in {minutesLeft:F0} minutes!");

                    foreach (var p in Player.ReadyList)
                    {
                        if (p != null && p.IsReady && !p.IsHost)
                            p.SendHint(warning, 6f);
                    }

                    Announcer.Message($"Warning . Automatic warhead detonation in {minutesLeft:F0} minutes", "", true, 0f, 1f);
                    Logger.Info($"[AutoNuke] Warning sent. {minutesLeft:F0} minutes until auto-nuke.");
                }

                // Auto-nuke
                if (!_nukeStarted && elapsed >= nukeSeconds)
                {
                    _nukeStarted = true;
                    StartAutoNuke();
                    yield break;
                }

                // Countdown hints in last 60 seconds
                float timeUntilNuke = nukeSeconds - elapsed;
                if (timeUntilNuke <= 60f && timeUntilNuke > 0f && !_nukeStarted)
                {
                    int secondsLeft = (int)timeUntilNuke;
                    if (secondsLeft % 10 == 0 || secondsLeft <= 10)
                    {
                        string countdown = HintBuilder.Size(
                            HintBuilder.Bold(HintBuilder.Color($"AUTO-NUKE IN {secondsLeft}s", "#ff3333")),
                            26);

                        foreach (var p in Player.ReadyList)
                        {
                            if (p != null && p.IsReady && !p.IsHost)
                                p.SendHint(countdown, 4f);
                        }
                    }
                }
            }
        }

        private void StartAutoNuke()
        {
            if (Warhead.IsDetonationInProgress || Warhead.IsDetonated)
                return;

            Warhead.Start(true, false, null);

            if (_config.LockAfterStart)
            {
                Warhead.IsLocked = true;
            }

            string banner = HintBuilder.WarningBanner(
                "AUTO-NUKE ACTIVATED",
                "The warhead has been automatically started!");

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(banner, 5f);
            }

            Announcer.Message("Automatic warhead detonation sequence engaged . All personnel evacuate immediately", "", true, 0f, 1f);
            Logger.Info("[AutoNuke] Auto-nuke activated!");
        }

        public void ForceStartNuke()
        {
            _nukeStarted = true;
            StartAutoNuke();
        }

        public void CancelAutoNuke()
        {
            _nukeStarted = true; // Prevent auto-start
            Timing.KillCoroutines(_nukeCoroutine);
            Logger.Info("[AutoNuke] Auto-nuke cancelled by admin.");
        }

        public float GetTimeUntilNuke()
        {
            if (!_roundInProgress) return -1f;
            float elapsed = (float)(DateTime.UtcNow - _roundStartTime).TotalSeconds;
            return (_config.AutoNukeTimeMinutes * 60f) - elapsed;
        }
    }
}
