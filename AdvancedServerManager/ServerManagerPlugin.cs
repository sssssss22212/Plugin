using System;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using AdvancedServerManager.Modules;

namespace AdvancedServerManager
{
    /// <summary>
    /// Advanced Server Manager — a complex LabAPI plugin providing:
    /// - Anti-AFK system (warnings, move to spectator, kick)
    /// - Auto Team Balance on respawn waves
    /// - Per-round Player Statistics tracking (kills, deaths, escapes, items)
    /// - Custom Spawn Loadouts per role
    /// - Welcome broadcasts
    /// - Admin commands for all modules
    /// </summary>
    public class ServerManagerPlugin : Plugin<PluginConfig>
    {
        public override string Name => "AdvancedServerManager";
        public override string Description => "All-in-one server management: Anti-AFK, Auto-Balance, Stats, Loadouts, Welcome messages";
        public override string Author => "Devin AI";
        public override Version Version => new Version(1, 0, 0, 0);
        public override Version RequiredApiVersion => new Version(LabApiProperties.CompiledVersion);
        public override LoadPriority Priority => LoadPriority.Medium;

        public static ServerManagerPlugin Instance { get; private set; }

        internal AntiAfkModule AntiAfkModule { get; private set; }
        internal AutoBalanceModule AutoBalanceModule { get; private set; }
        internal StatsModule StatsModule { get; private set; }
        internal LoadoutModule LoadoutModule { get; private set; }
        internal WelcomeModule WelcomeModule { get; private set; }

        public override void Enable()
        {
            Instance = this;

            AntiAfkModule = new AntiAfkModule(Config.AntiAfk);
            AutoBalanceModule = new AutoBalanceModule(Config.AutoBalance);
            StatsModule = new StatsModule(Config.Stats);
            LoadoutModule = new LoadoutModule(Config.Loadouts);
            WelcomeModule = new WelcomeModule(Config.Welcome);

            if (Config.AntiAfk.Enabled)
                AntiAfkModule.Enable();

            if (Config.AutoBalance.Enabled)
                AutoBalanceModule.Enable();

            if (Config.Stats.Enabled)
                StatsModule.Enable();

            if (Config.Loadouts.Enabled)
                LoadoutModule.Enable();

            if (Config.Welcome.Enabled)
                WelcomeModule.Enable();

            LabApi.Features.Console.Logger.Info($"[AdvancedServerManager] v{Version} loaded successfully!");
            LabApi.Features.Console.Logger.Info($"[AdvancedServerManager] Modules: AntiAFK={Config.AntiAfk.Enabled}, AutoBalance={Config.AutoBalance.Enabled}, Stats={Config.Stats.Enabled}, Loadouts={Config.Loadouts.Enabled}, Welcome={Config.Welcome.Enabled}");
        }

        public override void Disable()
        {
            AntiAfkModule?.Disable();
            AutoBalanceModule?.Disable();
            StatsModule?.Disable();
            LoadoutModule?.Disable();
            WelcomeModule?.Disable();

            Instance = null;

            LabApi.Features.Console.Logger.Info("[AdvancedServerManager] Plugin disabled.");
        }
    }
}
