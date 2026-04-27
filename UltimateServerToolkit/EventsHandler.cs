using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;

namespace UltimateServerToolkit
{
    /// <summary>
    /// Single hub that subscribes to LabAPI events on plugin enable and unsubscribes
    /// on disable. All callbacks dispatch to the appropriate module on <see cref="UstPlugin"/>.
    /// </summary>
    internal sealed class EventsHandler
    {
        private readonly UstPlugin _plugin;

        public EventsHandler(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Register()
        {
            PlayerEvents.Joined += OnJoined;
            PlayerEvents.Left += OnLeft;
            PlayerEvents.Hurting += OnHurting;
            PlayerEvents.Death += OnDeath;
            PlayerEvents.ChangingRole += OnChangingRole;
            PlayerEvents.Spawned += OnSpawned;

            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
        }

        public void Unregister()
        {
            PlayerEvents.Joined -= OnJoined;
            PlayerEvents.Left -= OnLeft;
            PlayerEvents.Hurting -= OnHurting;
            PlayerEvents.Death -= OnDeath;
            PlayerEvents.ChangingRole -= OnChangingRole;
            PlayerEvents.Spawned -= OnSpawned;

            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
        }

        private void OnJoined(PlayerJoinedEventArgs ev)
        {
            var profile = _plugin.Profiles.GetOrCreate(ev.Player);

            _plugin.DisplayNames.Apply(ev.Player);

            if (_plugin.Config.RoundLog.LogJoinsAndLeaves)
                _plugin.RoundLog.Append($"JOIN {ev.Player.UserId} '{ev.Player.Nickname}'");

            // Greet with karma + RP name reminder.
            try
            {
                var rpName = profile.HasRpName ? profile.RpName : "(not set — use .name)";
                ev.Player.SendHint(
                    $"<size=24><b>Welcome.</b></size>\nRP name: <color=#ffd27f>{rpName}</color>\nKarma: <b>{profile.Karma}</b>",
                    duration: 6f);
            }
            catch
            {
                // ignore hint failures during connect
            }
        }

        private void OnLeft(PlayerLeftEventArgs ev)
        {
            if (_plugin.Config.RoundLog.LogJoinsAndLeaves)
                _plugin.RoundLog.Append($"LEAVE {ev.Player.UserId} '{ev.Player.Nickname}'");

            _plugin.ProfileStore.SaveAll();
        }

        private void OnHurting(PlayerHurtingEventArgs ev)
        {
            if (!ev.IsAllowed) return;
            _plugin.AntiRdm.OnHurting(ev.Player, ev.Attacker);
        }

        private void OnDeath(PlayerDeathEventArgs ev)
        {
            _plugin.AntiRdm.OnDeath(ev.Player, ev.Attacker);

            if (_plugin.Config.RoundLog.LogDeaths)
            {
                var attacker = ev.Attacker?.Nickname ?? "(none)";
                _plugin.RoundLog.Append(
                    $"DEATH {ev.Player.Nickname} <- {attacker} ({ev.DamageHandler?.GetType().Name})");
            }
        }

        private void OnChangingRole(PlayerChangingRoleEventArgs ev)
        {
            if (!_plugin.Whitelist.IsAllowed(ev.Player, ev.NewRole))
            {
                ev.IsAllowed = false;
                try
                {
                    ev.Player.SendHint(_plugin.Config.Whitelist.DenyMessage, duration: 5f);
                }
                catch
                {
                    // ignore
                }
                _plugin.RoundLog.Append(
                    $"WHITELIST_DENY {ev.Player.Nickname} -> {ev.NewRole}");
            }
        }

        private void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            _plugin.DisplayNames.Apply(ev.Player);

            if (_plugin.Config.RoundLog.LogRoleChanges)
                _plugin.RoundLog.Append($"SPAWN {ev.Player.Nickname} -> {ev.Player.Role}");
        }

        private void OnWaitingForPlayers()
        {
            _plugin.AntiRdm.Reset();
            _plugin.Notes.Reset();
        }

        private void OnRoundStarted()
        {
            _plugin.RoundLog.StartRound();
        }

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            _plugin.Karma.OnRoundEnded();
            _plugin.RoundLog.EndRound($"WinningTeam={ev.LeadingTeam}");
            _plugin.ProfileStore.SaveAll();
        }
    }
}
