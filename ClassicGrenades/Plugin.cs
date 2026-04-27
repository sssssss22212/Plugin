using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using PlayerStatsSystem;

namespace ClassicGrenades
{
    /// <summary>
    /// Single-purpose plugin that restores "classic" team-protected grenade rules:
    /// teammates are immune to each other's HE / SCP-018 explosions, but the
    /// thrower and any enemies still take full damage.
    /// </summary>
    public sealed class ClassicGrenadesPlugin : Plugin<Config>
    {
        public override string Name => "ClassicGrenades";
        public override string Description =>
            "Classic-server FF rules for grenades: teammates immune, thrower and enemies still hit.";
        public override string Author => "sssssss22212";
        public override Version Version => new Version(1, 0, 0, 0);
        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);

        public override void Enable()
        {
            if (!Config.IsEnabled)
            {
                Logger.Info("[ClassicGrenades] Disabled in config.");
                return;
            }

            PlayerEvents.Hurting += OnHurting;
            Logger.Info($"[ClassicGrenades] Enabled. HE-protect={Config.ProtectTeammatesFromHeGrenades}, " +
                        $"018-protect={Config.ProtectTeammatesFromScp018}, " +
                        $"throwerSelfDamage={Config.ThrowerSelfDamage}");
        }

        public override void Disable()
        {
            PlayerEvents.Hurting -= OnHurting;
            Logger.Info("[ClassicGrenades] Disabled.");
        }

        private void OnHurting(PlayerHurtingEventArgs ev)
        {
            // Already cancelled by another plugin/system — nothing to do.
            if (!ev.IsAllowed) return;

            // Only handle grenade-style explosions.
            bool isHe = ev.DamageHandler is ExplosionDamageHandler;
            bool is018 = ev.DamageHandler is Scp018DamageHandler;
            if (!isHe && !is018) return;

            if (isHe && !Config.ProtectTeammatesFromHeGrenades) return;
            if (is018 && !Config.ProtectTeammatesFromScp018) return;

            // No attacker (e.g. environmental) → leave vanilla behaviour.
            var attacker = ev.Attacker;
            var victim = ev.Player;
            if (attacker == null || victim == null) return;

            // Self damage — gated by config.
            if (attacker.UserId == victim.UserId)
            {
                if (!Config.ThrowerSelfDamage)
                {
                    ev.IsAllowed = false;
                    if (Config.LogBlockedHits)
                        Logger.Info($"[ClassicGrenades] Blocked self damage for {attacker.Nickname} ({DamageKindLabel(isHe)}).");
                }
                return;
            }

            // Different factions/teams → enemy, allow vanilla damage.
            if (attacker.Faction != victim.Faction) return;

            // Same faction & not self → cancel friendly grenade damage.
            ev.IsAllowed = false;

            if (Config.LogBlockedHits)
            {
                Logger.Info(
                    $"[ClassicGrenades] Blocked friendly {DamageKindLabel(isHe)}: " +
                    $"{attacker.Nickname} -> {victim.Nickname} (Faction={attacker.Faction})");
            }
        }

        private static string DamageKindLabel(bool isHe) => isHe ? "HE-grenade" : "SCP-018";
    }
}
