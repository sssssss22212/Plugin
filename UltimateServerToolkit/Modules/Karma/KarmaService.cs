using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Modules.Karma
{
    public sealed class KarmaService
    {
        private readonly UstPlugin _plugin;

        public KarmaService(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Penalize(Player p, int amount, string reason)
        {
            if (!_plugin.Config.Karma.Enabled || p == null || amount <= 0) return;

            var prof = _plugin.Profiles.GetOrCreate(p);
            prof.Karma -= amount;

            Logger.Info($"[UST/Karma] {p.Nickname} -{amount} ({reason}). Сейчас: {prof.Karma}");

            try
            {
                p.SendHint(
                    $"<color=#ff5555>-{amount} кармы:</color> <i>{reason}</i> (<b>{prof.Karma}</b>)",
                    duration: 4f);
            }
            catch { }
        }

        public void Reward(Player p, int amount, string reason)
        {
            if (!_plugin.Config.Karma.Enabled || p == null || amount <= 0) return;

            var prof = _plugin.Profiles.GetOrCreate(p);
            prof.Karma += amount;

            Logger.Info($"[UST/Karma] {p.Nickname} +{amount} ({reason}). Сейчас: {prof.Karma}");
        }

        public void OnRoundEnded()
        {
            if (!_plugin.Config.Karma.Enabled) return;
            var bonus = _plugin.Config.Karma.RoundEndBonus;

            foreach (var p in Player.ReadyList)
            {
                if (p == null || !p.IsPlayer) continue;

                if (bonus > 0)
                    Reward(p, bonus, "Пережил раунд");

                if (!_plugin.Profiles.TryGet(p.UserId, out var prof)) continue;

                if (_plugin.Config.Karma.KickOnCritical &&
                    prof.Karma <= _plugin.Config.Karma.CriticalKarmaThreshold)
                {
                    Server.KickPlayer(p, $"Карма ниже критической ({prof.Karma}). Исправься и возвращайся.");
                }
            }

            _plugin.ProfileStore.SaveAll();
        }
    }
}
