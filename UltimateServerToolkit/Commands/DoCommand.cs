using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class DoCommand : ICommand
    {
        public string Command => "do";
        public string[] Aliases => new[] { "rpdo" };
        public string Description => "Narrate the environment. Usage: .do <text>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "RP commands disabled."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var text = CommandUtil.Join(arguments).Trim();
            if (string.IsNullOrEmpty(text))
            {
                response = "Usage: .do <text>";
                return false;
            }

            var msg = plugin.Config.RpCommands.DoFormat.Replace("{action}", text);
            CommandUtil.BroadcastNearby(player, plugin.Config.RpCommands.MeRangeMeters, msg);
            plugin.RoundLog.Append($"DO {player.Nickname}: {text}");

            response = "Narration sent.";
            return true;
        }
    }
}
