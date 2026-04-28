using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Plugins;
using PlayerStatsSystem;

namespace ClassicGrenades
{
    public sealed class ClassicGrenadesPlugin : Plugin<Config>
    {
        public override string Name => "HE";
        public override string Description => "Гранаты не бьют тиммейтов, но бьют кидавшего и врагов.";
        public override string Author => "SteamTime";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 1, 6);

        public override void Enable()
        {
            if (!Config.IsEnabled) return;
            PlayerEvents.Hurting += OnHurt;
        }

        public override void Disable()
        {
            PlayerEvents.Hurting -= OnHurt;
        }

        private void OnHurt(PlayerHurtingEventArgs ev)
        {
            if (!ev.IsAllowed) return;

            var dh = ev.DamageHandler;
            bool he = dh is ExplosionDamageHandler;
            bool s018 = dh is Scp018DamageHandler;

            if (he && !Config.BlockHe) return;
            if (s018 && !Config.Block018) return;
            if (!he && !s018) return;

            var att = ev.Attacker;
            var vic = ev.Player;
            if (att == null || vic == null) return;

            if (att.UserId == vic.UserId)
            {
                if (!Config.SelfDmg) ev.IsAllowed = false;
                return;
            }

            if (att.Faction == vic.Faction) ev.IsAllowed = false;
        }
    }
}
