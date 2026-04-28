using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Plugins;
using PlayerStatsSystem;

namespace HePlugin
{
    public sealed class HePlugin : Plugin<Config>
    {
        public override string Name => "HE";
        public override string Description => "";
        public override string Author => "SteamTime";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 1, 6);

        public override void Enable()
        {
            if (!Config.IsEnabled) return;
            PlayerEvents.Hurting += OnHurting;
        }

        public override void Disable()
        {
            PlayerEvents.Hurting -= OnHurting;
        }

        private void OnHurting(PlayerHurtingEventArgs args)
        {
            if (!args.IsAllowed) return;

            var dmgHandler = args.DamageHandler;
            bool heDmg = dmgHandler is ExplosionDamageHandler;
            bool scp018Dmg = dmgHandler is Scp018DamageHandler;

            if (heDmg && !Config.BlockHe) return;
            if (scp018Dmg && !Config.Block018) return;
            if (!heDmg && !scp018Dmg) return;

            var attacker = args.Attacker;
            var victim = args.Player;
            if (attacker == null || victim == null) return;

            if (attacker.UserId == victim.UserId)
            {
                if (!Config.SelfDmg) args.IsAllowed = false;
                return;
            }

            if (attacker.Faction == victim.Faction) args.IsAllowed = false;
        }
    }
}
