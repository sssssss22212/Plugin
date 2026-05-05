using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Plugins;
using PlayerStatsSystem;

namespace ClassicGrenades
{
    public sealed class ClassicGrenadesPlugin : Plugin<Config>
    {
        public override string Name => "ClassicGrenades";
        public override string Description => "Classic-style grenades: no teammate damage, owner still takes self-damage.";
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

            var dmg = args.DamageHandler;
            bool heDmg = dmg is ExplosionDamageHandler;
            bool scp018Dmg = dmg is Scp018DamageHandler;

            if (!heDmg && !scp018Dmg) return;
            if (heDmg && !Config.BlockHe) return;
            if (scp018Dmg && !Config.Block018) return;

            var attacker = args.Attacker;
            var victim = args.Player;
            if (victim == null) return;

            if (attacker == null)
            {
                if (Config.BlockOrphan) args.IsAllowed = false;
                return;
            }

            if (attacker.PlayerId == victim.PlayerId)
            {
                if (!Config.OwnerSelfDamage) args.IsAllowed = false;
                return;
            }

            if (attacker.Team == victim.Team || attacker.Faction == victim.Faction)
                args.IsAllowed = false;
        }
    }
}
