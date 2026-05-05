using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Plugins;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;
using PlayerRoles;
using Footprinting;

namespace ClassicGrenades
{
    public sealed class ClassicGrenadesPlugin : Plugin<Config>
    {
        public override string Name => "ClassicGrenades";
        public override string Description => "";
        public override string Author => "SteamTime";
        public override Version Version => new Version(1, 0, 2);
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

            var victim = args.Player;
            if (victim == null) return;

            Footprint fp = default;
            bool hasFp = false;
            if (dmg is AttackerDamageHandler att)
            {
                fp = att.Attacker;
                hasFp = fp.Hub != null;
            }

            int attackerId = -1;
            Team attackerTeam = Team.Dead;
            Faction attackerFaction = Faction.Unclassified;

            if (hasFp)
            {
                attackerId = fp.Hub.PlayerId;
                attackerTeam = fp.Role.GetTeam();
                attackerFaction = attackerTeam.GetFaction();
            }
            else if (args.Attacker != null)
            {
                attackerId = args.Attacker.PlayerId;
                attackerTeam = args.Attacker.Team;
                attackerFaction = args.Attacker.Faction;
            }

            if (Config.Debug)
            {
                Logger.Info($"[CG] hit type={dmg.GetType().Name} fp={hasFp} attacker=[id={attackerId} team={attackerTeam} faction={attackerFaction}] victim=[id={victim.PlayerId} team={victim.Team} faction={victim.Faction}]");
            }

            if (attackerId == -1)
            {
                if (Config.BlockOrphan) args.IsAllowed = false;
                if (Config.Debug) Logger.Info($"[CG] orphan -> blocked={!args.IsAllowed}");
                return;
            }

            if (attackerId == victim.PlayerId)
            {
                if (!Config.OwnerSelfDamage) args.IsAllowed = false;
                if (Config.Debug) Logger.Info($"[CG] self -> blocked={!args.IsAllowed}");
                return;
            }

            bool sameTeam = attackerTeam == victim.Team;
            bool sameFaction = attackerFaction == victim.Faction;
            if (sameTeam || sameFaction)
            {
                args.IsAllowed = false;
                if (Config.Debug) Logger.Info($"[CG] teammate sameTeam={sameTeam} sameFaction={sameFaction} -> BLOCKED");
            }
            else if (Config.Debug)
            {
                Logger.Info($"[CG] enemy sameTeam={sameTeam} sameFaction={sameFaction} -> allowed");
            }
        }
    }
}
