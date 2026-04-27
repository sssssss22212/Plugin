using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using PlayerStatsSystem;

namespace ClassicGrenades
{
    public sealed class ClassicGrenadesPlugin : Plugin<Config>
    {
        public override string Name => "ClassicGrenades";
        public override string Description => "Классика: гранаты не бьют тиммейтов, но бьют кидавшего и врагов.";
        public override string Author => "sssssss22212";
        public override Version Version => new Version(1, 0, 0, 0);
        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);

        public override void Enable()
        {
            if (!Config.On)
            {
                Logger.Info("[Гранаты] Выключено в конфиге.");
                return;
            }

            PlayerEvents.Hurting += OnHurt;
            Logger.Info($"[Гранаты] Включено. HE={Config.BlockHe}, 018={Config.Block018}, селф-урон={Config.SelfDmg}");
        }

        public override void Disable()
        {
            PlayerEvents.Hurting -= OnHurt;
            Logger.Info("[Гранаты] Выключено.");
        }

        private void OnHurt(PlayerHurtingEventArgs ev)
        {
            if (!ev.IsAllowed) return;

            bool he = ev.DamageHandler is ExplosionDamageHandler;
            bool s018 = ev.DamageHandler is Scp018DamageHandler;
            if (!he && !s018) return;
            if (he && !Config.BlockHe) return;
            if (s018 && !Config.Block018) return;

            var att = ev.Attacker;
            var vic = ev.Player;
            if (att == null || vic == null) return;

            if (att.UserId == vic.UserId)
            {
                if (!Config.SelfDmg)
                {
                    ev.IsAllowed = false;
                    if (Config.LogBlocks)
                        Logger.Info($"[Гранаты] Заблокирован селф-урон: {att.Nickname} ({Kind(he)}).");
                }
                return;
            }

            if (att.Faction != vic.Faction) return;

            ev.IsAllowed = false;
            if (Config.LogBlocks)
                Logger.Info($"[Гранаты] {att.Nickname} -> {vic.Nickname} ({Kind(he)}, фракция {att.Faction}) — урон отменён.");
        }

        private static string Kind(bool he) => he ? "HE-граната" : "SCP-018";
    }
}
