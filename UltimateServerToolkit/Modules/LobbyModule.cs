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
    public class LobbyModule
    {
        private readonly LobbyConfig _config;
        private bool _isLobby;
        private CoroutineHandle _lobbyCoroutine;

        public LobbyModule(LobbyConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            PlayerEvents.Joined += OnPlayerJoined;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundStarted += OnRoundStarted;
            Logger.Info("[Lobby] Module enabled.");
        }

        public void Disable()
        {
            PlayerEvents.Joined -= OnPlayerJoined;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundStarted -= OnRoundStarted;
            Timing.KillCoroutines(_lobbyCoroutine);
            Logger.Info("[Lobby] Module disabled.");
        }

        private void OnWaitingForPlayers()
        {
            _isLobby = true;
            _lobbyCoroutine = Timing.RunCoroutine(LobbyHintLoop());
        }

        private void OnRoundStarted()
        {
            _isLobby = false;
            Timing.KillCoroutines(_lobbyCoroutine);
        }

        private void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            if (ev.Player == null || !ev.Player.IsReady || ev.Player.IsHost)
                return;

            string welcome = _config.WelcomeMessage.Replace("%player%", ev.Player.Nickname);

            // Send personalized welcome
            Timing.CallDelayed(1f, () =>
            {
                if (ev.Player != null && ev.Player.IsReady)
                    ev.Player.SendHint(welcome, _config.MessageDuration);
            });

            // Notify others
            string joinMsg = HintBuilder.Size(
                HintBuilder.Color($"+ ", "#00ff00") +
                HintBuilder.Color(ev.Player.Nickname, "#ffffff") +
                HintBuilder.Color(" joined the server", "#aaaaaa"),
                14);

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost && p.UserId != ev.Player.UserId)
                    p.SendHint(joinMsg, 3f);
            }

            Logger.Info($"[Lobby] {ev.Player.Nickname} joined the server.");
        }

        private IEnumerator<float> LobbyHintLoop()
        {
            while (_isLobby)
            {
                yield return Timing.WaitForSeconds(3f);

                int playerCount = Server.PlayerCount;
                string lobbyHint = BuildLobbyHint(playerCount);

                foreach (var p in Player.ReadyList)
                {
                    if (p != null && p.IsReady && !p.IsHost)
                        p.SendHint(lobbyHint, 3.5f);
                }
            }
        }

        private string BuildLobbyHint(int playerCount)
        {
            var lines = new List<string>();

            lines.Add(HintBuilder.Header("LOBBY", "#00ffcc", 30));
            lines.Add(HintBuilder.Separator("#00aa88"));
            lines.Add("");
            lines.Add(HintBuilder.Size(
                HintBuilder.Color("Waiting for players...", "#aaaaaa"),
                20));
            lines.Add("");
            lines.Add(HintBuilder.Size(
                HintBuilder.Bold(HintBuilder.Color($"Players: {playerCount}/{Server.MaxPlayers}", "#ffffff")),
                22));
            lines.Add("");

            // Animated dots
            string dots = new string('.', (int)(MEC.Timing.LocalTime % 4) + 1);
            lines.Add(HintBuilder.Size(
                HintBuilder.Color($"Loading{dots}", "#888888"),
                16));

            lines.Add("");
            lines.Add(HintBuilder.Separator("#00aa88"));
            lines.Add(HintBuilder.Small("Powered by UltimateServerToolkit", "#555555"));

            return string.Join("\n", lines);
        }
    }
}
