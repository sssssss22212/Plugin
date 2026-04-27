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
        public string Description => "Шёпот в радиусе ближнего боя. Использование: .whisper <текст>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "РП-команды выключены."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var text = CommandUtil.Join(arguments).Trim();
            if (string.IsNullOrEmpty(text)) { response = "Использование: .whisper <текст>"; return false; }

            var name = MeCommand.ResolveName(plugin, p);
            var msg = plugin.Config.RpCommands.WhisperFormat.Replace("{name}", name).Replace("{message}", text);

            CommandUtil.BroadcastNearby(p, plugin.Config.RpCommands.WhisperRangeMeters, msg);
            plugin.RoundLog.Append($"WHISPER {p.Nickname}: {text}");

            response = "Шёпот отправлен.";
            return true;
        }
    }
}
