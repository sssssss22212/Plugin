using System;
using System.Collections.Generic;
using CommandSystem;
using LabApi.Features.Wrappers;
using UltimateServerToolkit.Modules;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Commands
{
    public class StatsCommand : ICommand
    {
        private readonly KillstreakModule _module;

        public StatsCommand(KillstreakModule module)
        {
            _module = module;
        }

        public string Command => "stats";
        public string[] Aliases => new[] { "mystats", "kd" };
        public string Description => "View your kill/death stats for this round.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = null;
            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.ReferenceHub != null && p.ReferenceHub.authManager != null)
                {
                    var cmdSender = sender as CommandSender;
                    if (cmdSender != null && p.Nickname == cmdSender.LogName)
                    {
                        player = p;
                        break;
                    }
                }
            }

            if (player == null)
            {
                // Try to find by sender name
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
            }

            if (player == null)
            {
                response = "Could not identify you. Try again in-game.";
                return false;
            }

            var data = _module.GetPlayerData(player.UserId);

            if (data == null)
            {
                string noDataHint = HintBuilder.InfoPanel("YOUR STATS", new[]
                {
                    "No kills or deaths yet this round!",
                    "Get out there and fight!"
                }, "#00ffcc");

                player.SendHint(noDataHint, 5f);
                response = "No stats yet this round.";
                return true;
            }

            float kd = data.TotalDeaths > 0 ? (float)data.TotalKills / data.TotalDeaths : data.TotalKills;

            string[] lines = new[]
            {
                HintBuilder.StatLine("Kills", data.TotalKills.ToString(), "#aaaaaa", "#00ff00"),
                HintBuilder.StatLine("Deaths", data.TotalDeaths.ToString(), "#aaaaaa", "#ff3333"),
                HintBuilder.StatLine("K/D Ratio", kd.ToString("F2"), "#aaaaaa", "#ffcc00"),
                HintBuilder.StatLine("Current Streak", data.CurrentStreak.ToString(), "#aaaaaa", "#ff6600"),
                HintBuilder.StatLine("Best Streak", data.BestStreak.ToString(), "#aaaaaa", "#ff00ff")
            };

            string hint = HintBuilder.InfoPanel("YOUR STATS", lines, "#00ffcc");
            player.SendHint(hint, 6f);

            response = $"Kills: {data.TotalKills} | Deaths: {data.TotalDeaths} | K/D: {kd:F2} | Best Streak: {data.BestStreak}";
            return true;
        }
    }
}
