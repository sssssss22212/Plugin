using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Modules.Karma
{
    /// <summary>
    /// Adjusts player karma in response to gameplay events. Owns no state of its own —
    /// reads/writes the karma value on <see cref="Profiles.RpProfile"/>.
    /// </summary>
    public sealed class KarmaService
    {
        private readonly UstPlugin _plugin;

        public KarmaService(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Penalize(Player player, int amount, string reason)
        {
            if (!_plugin.Config.Karma.Enabled || player == null || amount <= 0) return;

            var profile = _plugin.Profiles.GetOrCreate(player);
            profile.Karma -= amount;

            Logger.Info($"[UST/Karma] {player.Nickname} -{amount} karma ({reason}). New: {profile.Karma}");

            try
            {
                player.SendHint(
                    $"<color=#ff5555>-{amount} karma:</color> <i>{reason}</i> (<b>{profile.Karma}</b>)",
                    duration: 4f);
            }
            catch
            {
                // hint pipeline may not be ready (e.g., during disconnection); ignore.
            }
        }

        public void Reward(Player player, int amount, string reason)
        {
            if (!_plugin.Config.Karma.Enabled || player == null || amount <= 0) return;

            var profile = _plugin.Profiles.GetOrCreate(player);
            profile.Karma += amount;

            Logger.Info($"[UST/Karma] {player.Nickname} +{amount} karma ({reason}). New: {profile.Karma}");
        }

        public void OnRoundEnded()
        {
            if (!_plugin.Config.Karma.Enabled) return;

            var bonus = _plugin.Config.Karma.RoundEndBonus;

            foreach (var player in Player.ReadyList)
            {
                if (player == null || !player.IsPlayer) continue;

                if (bonus > 0)
                    Reward(player, bonus, "Round survived");

                if (!_plugin.Profiles.TryGet(player.UserId, out var profile)) continue;

                if (_plugin.Config.Karma.KickOnCritical &&
                    profile.Karma <= _plugin.Config.Karma.CriticalKarmaThreshold)
                {
                    Server.KickPlayer(
                        player,
                        $"Karma below critical threshold ({profile.Karma}). Improve and come back.");
                }
            }

            _plugin.ProfileStore.SaveAll();
        }
    }
}
