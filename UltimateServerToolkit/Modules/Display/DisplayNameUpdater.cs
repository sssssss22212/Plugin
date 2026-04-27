using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Modules.Display
{
    public sealed class DisplayNameUpdater
    {
        private readonly UstPlugin _plugin;

        public DisplayNameUpdater(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Apply(Player p)
        {
            if (p == null || !_plugin.Config.Profiles.OverrideDisplayName) return;
            if (!_plugin.Profiles.TryGet(p.UserId, out var prof)) return;
            p.DisplayName = prof.HasRpName ? prof.RpName : p.Nickname;
        }

        public void Reset(Player p)
        {
            if (p == null) return;
            p.DisplayName = p.Nickname;
        }
    }
}
