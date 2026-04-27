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
        public string Description => "Описать обстановку. Использование: .do <текст>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "РП-команды выключены."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var text = CommandUtil.Join(arguments).Trim();
            if (string.IsNullOrEmpty(text)) { response = "Использование: .do <текст>"; return false; }

            var msg = plugin.Config.RpCommands.DoFormat.Replace("{action}", text);
            CommandUtil.BroadcastNearby(p, plugin.Config.RpCommands.MeRangeMeters, msg);
            plugin.RoundLog.Append($"DO {p.Nickname}: {text}");

            response = "Отправлено.";
            return true;
        }
    }
}
