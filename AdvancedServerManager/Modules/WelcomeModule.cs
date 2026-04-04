using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;

namespace AdvancedServerManager.Modules
{
    /// <summary>
    /// Welcome module that sends a configurable broadcast to players when they join.
    /// Supports %player% placeholder for the player's nickname.
    /// </summary>
    public class WelcomeModule
    {
        private readonly WelcomeConfig _config;

        public WelcomeModule(WelcomeConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            PlayerEvents.Joined += OnPlayerJoined;
            LabApi.Features.Console.Logger.Info("[Welcome] Module enabled.");
        }

        public void Disable()
        {
            PlayerEvents.Joined -= OnPlayerJoined;
            LabApi.Features.Console.Logger.Info("[Welcome] Module disabled.");
        }

        private void OnPlayerJoined(PlayerJoinedEventArgs args)
        {
            if (args.Player == null) return;

            string message = _config.Message.Replace("%player%", args.Player.Nickname);
            args.Player.SendBroadcast(message, _config.Duration);
        }
    }
}
