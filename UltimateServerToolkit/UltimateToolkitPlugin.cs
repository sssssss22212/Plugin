using System;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;

namespace UltimateServerToolkit
{
    public class UltimateToolkitPlugin : Plugin<ToolkitConfig>
    {
        public override string Name => "UltimateServerToolkit";
        public override string Description => "All-in-one server toolkit: HUD, Killstreaks, AutoNuke, SCP Info, Respawn Timer, Door Lockdown, CASSIE Announcements, Admin Tools, Lobby System, Reports";
        public override string Author => "Devin AI";
        public override Version Version => new Version(2, 0, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 1, 6, 0);

        internal Modules.HudModule HudModule { get; private set; }
        internal Modules.KillstreakModule KillstreakModule { get; private set; }
        internal Modules.AutoNukeModule AutoNukeModule { get; private set; }
        internal Modules.ScpInfoModule ScpInfoModule { get; private set; }
        internal Modules.RespawnTimerModule RespawnTimerModule { get; private set; }
        internal Modules.DoorLockdownModule DoorLockdownModule { get; private set; }
        internal Modules.CassieAutoModule CassieAutoModule { get; private set; }
        internal Modules.AdminToolsModule AdminToolsModule { get; private set; }
        internal Modules.LobbyModule LobbyModule { get; private set; }
        internal Modules.PlayerReportsModule PlayerReportsModule { get; private set; }

        public override void Enable()
        {
            Logger.Info("===========================================");
            Logger.Info("  UltimateServerToolkit v2.0.0 Loading...");
            Logger.Info("===========================================");

            HudModule = new Modules.HudModule(Config.Hud);
            KillstreakModule = new Modules.KillstreakModule(Config.Killstreaks);
            AutoNukeModule = new Modules.AutoNukeModule(Config.AutoNuke);
            ScpInfoModule = new Modules.ScpInfoModule(Config.ScpInfo);
            RespawnTimerModule = new Modules.RespawnTimerModule(Config.RespawnTimer);
            DoorLockdownModule = new Modules.DoorLockdownModule(Config.DoorLockdown);
            CassieAutoModule = new Modules.CassieAutoModule(Config.CassieAuto);
            AdminToolsModule = new Modules.AdminToolsModule(Config.AdminTools);
            LobbyModule = new Modules.LobbyModule(Config.Lobby);
            PlayerReportsModule = new Modules.PlayerReportsModule(Config.Reports);

            if (Config.Hud.Enabled) HudModule.Enable();
            if (Config.Killstreaks.Enabled) KillstreakModule.Enable();
            if (Config.AutoNuke.Enabled) AutoNukeModule.Enable();
            if (Config.ScpInfo.Enabled) ScpInfoModule.Enable();
            if (Config.RespawnTimer.Enabled) RespawnTimerModule.Enable();
            if (Config.DoorLockdown.Enabled) DoorLockdownModule.Enable();
            if (Config.CassieAuto.Enabled) CassieAutoModule.Enable();
            if (Config.AdminTools.Enabled) AdminToolsModule.Enable();
            if (Config.Lobby.Enabled) LobbyModule.Enable();
            if (Config.Reports.Enabled) PlayerReportsModule.Enable();

            RegisterCommands();

            Logger.Info("===========================================");
            Logger.Info("  UltimateServerToolkit v2.0.0 Loaded!");
            Logger.Info($"  Modules: {CountEnabledModules()}/10 enabled");
            Logger.Info("===========================================");
        }

        public override void Disable()
        {
            if (Config.Hud.Enabled) HudModule?.Disable();
            if (Config.Killstreaks.Enabled) KillstreakModule?.Disable();
            if (Config.AutoNuke.Enabled) AutoNukeModule?.Disable();
            if (Config.ScpInfo.Enabled) ScpInfoModule?.Disable();
            if (Config.RespawnTimer.Enabled) RespawnTimerModule?.Disable();
            if (Config.DoorLockdown.Enabled) DoorLockdownModule?.Disable();
            if (Config.CassieAuto.Enabled) CassieAutoModule?.Disable();
            if (Config.AdminTools.Enabled) AdminToolsModule?.Disable();
            if (Config.Lobby.Enabled) LobbyModule?.Disable();
            if (Config.Reports.Enabled) PlayerReportsModule?.Disable();

            Logger.Info("UltimateServerToolkit disabled.");
        }

        private void RegisterCommands()
        {
            var raHandler = LabApi.Features.Wrappers.Server.RemoteAdminCommandHandler;
            var clientHandler = LabApi.Features.Wrappers.Server.ClientCommandHandler;

            raHandler.RegisterCommand(new Commands.LockdownCommand(DoorLockdownModule));
            raHandler.RegisterCommand(new Commands.NukeCommand(AutoNukeModule));
            raHandler.RegisterCommand(new Commands.HealCommand());
            raHandler.RegisterCommand(new Commands.TpCommand());
            raHandler.RegisterCommand(new Commands.SizeCommand());
            raHandler.RegisterCommand(new Commands.GravityCommand());
            raHandler.RegisterCommand(new Commands.EffectAllCommand());

            clientHandler.RegisterCommand(new Commands.StatsCommand(KillstreakModule));
            clientHandler.RegisterCommand(new Commands.ReportCommand(PlayerReportsModule));
            clientHandler.RegisterCommand(new Commands.ServerInfoCommand(Config));

            Logger.Info("[Commands] Registered 10 custom commands (7 RA + 3 client).");
        }

        private int CountEnabledModules()
        {
            int count = 0;
            if (Config.Hud.Enabled) count++;
            if (Config.Killstreaks.Enabled) count++;
            if (Config.AutoNuke.Enabled) count++;
            if (Config.ScpInfo.Enabled) count++;
            if (Config.RespawnTimer.Enabled) count++;
            if (Config.DoorLockdown.Enabled) count++;
            if (Config.CassieAuto.Enabled) count++;
            if (Config.AdminTools.Enabled) count++;
            if (Config.Lobby.Enabled) count++;
            if (Config.Reports.Enabled) count++;
            return count;
        }
    }
}
