using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Modules.Display
{
    /// <summary>
    /// Pushes the player's RP name (or original nickname) into the in-game
    /// <c>DisplayName</c> shown above the head and in the player list.
    /// </summary>
    public sealed class DisplayNameUpdater
    {
        private readonly UstPlugin _plugin;

        public DisplayNameUpdater(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Apply(Player player)
        {
            if (player == null || !_plugin.Config.Profiles.OverrideDisplayName) return;
            if (!_plugin.Profiles.TryGet(player.UserId, out var profile)) return;

            player.DisplayName = profile.HasRpName ? profile.RpName : player.Nickname;
        }

        public void Reset(Player player)
        {
            if (player == null) return;
            player.DisplayName = player.Nickname;
        }
    }
}
