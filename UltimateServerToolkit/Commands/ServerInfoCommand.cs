using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Commands
{
    public class ServerInfoCommand : ICommand
    {
        private readonly ToolkitConfig _config;

        public ServerInfoCommand(ToolkitConfig config)
        {
            _config = config;
        }

        public string Command => "serverinfo";
        public string[] Aliases => new[] { "si", "info" };
        public string Description => "View server information and active modules.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = null;
            var cmdSender = sender as CommandSender;
            string senderName = cmdSender?.LogName ?? "";

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.Nickname == senderName)
                {
                    player = p;
                    break;
                }
            }

            int moduleCount = 0;
            if (_config.Hud.Enabled) moduleCount++;
            if (_config.Killstreaks.Enabled) moduleCount++;
            if (_config.AutoNuke.Enabled) moduleCount++;
            if (_config.ScpInfo.Enabled) moduleCount++;
            if (_config.RespawnTimer.Enabled) moduleCount++;
            if (_config.DoorLockdown.Enabled) moduleCount++;
            if (_config.CassieAuto.Enabled) moduleCount++;
            if (_config.AdminTools.Enabled) moduleCount++;
            if (_config.Lobby.Enabled) moduleCount++;
            if (_config.Reports.Enabled) moduleCount++;

            string[] lines = new[]
            {
                HintBuilder.StatLine("Server", _config.Hud.ServerDisplayName, "#aaaaaa", "#00ffcc"),
                HintBuilder.StatLine("Players", $"{Server.PlayerCount}/{Server.MaxPlayers}", "#aaaaaa", "#ffffff"),
                HintBuilder.StatLine("TPS", $"{Server.Tps:F0}", "#aaaaaa", Server.Tps > 55 ? "#00ff00" : "#ffcc00"),
                HintBuilder.StatLine("Modules", $"{moduleCount}/10 active", "#aaaaaa", "#00ffcc"),
                "",
                HintBuilder.Color("Active Features:", "#ffcc00"),
                ModuleStatus("HUD", _config.Hud.Enabled),
                ModuleStatus("Killstreaks", _config.Killstreaks.Enabled),
                ModuleStatus("AutoNuke", _config.AutoNuke.Enabled),
                ModuleStatus("SCP Info", _config.ScpInfo.Enabled),
                ModuleStatus("Respawn Timer", _config.RespawnTimer.Enabled),
                ModuleStatus("Door Lockdown", _config.DoorLockdown.Enabled),
                ModuleStatus("CASSIE Auto", _config.CassieAuto.Enabled),
                ModuleStatus("Admin Tools", _config.AdminTools.Enabled),
                ModuleStatus("Lobby", _config.Lobby.Enabled),
                ModuleStatus("Reports", _config.Reports.Enabled)
            };

            if (player != null)
            {
                string hint = HintBuilder.InfoPanel("SERVER INFO", lines, "#00ffcc");
                player.SendHint(hint, 8f);
            }

            response = $"UltimateServerToolkit v2.0.0 | {moduleCount}/10 modules active | {Server.PlayerCount}/{Server.MaxPlayers} players | TPS: {Server.Tps:F0}";
            return true;
        }

        private string ModuleStatus(string name, bool enabled)
        {
            string status = enabled
                ? HintBuilder.Color("[ON]", "#00ff00")
                : HintBuilder.Color("[OFF]", "#ff3333");
            return $"  {status} {HintBuilder.Color(name, "#cccccc")}";
        }
    }
}
