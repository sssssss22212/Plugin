using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class WhisperCommand : ICommand
    {
        public string Command => "whisper";
        public string[] Aliases => new[] { "w", "rpw" };
        public string Description => "Whisper to anyone in melee range. Usage: .whisper <message>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "RP commands disabled."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var text = CommandUtil.Join(arguments).Trim();
            if (string.IsNullOrEmpty(text))
            {
                response = "Usage: .whisper <message>";
                return false;
            }

            var name = MeCommand.ResolveName(plugin, player);
            var msg = plugin.Config.RpCommands.WhisperFormat
                .Replace("{name}", name)
                .Replace("{message}", text);

            CommandUtil.BroadcastNearby(player, plugin.Config.RpCommands.WhisperRangeMeters, msg);
            plugin.RoundLog.Append($"WHISPER {player.Nickname}: {text}");

            response = "Whisper sent.";
            return true;
        }
    }
}
