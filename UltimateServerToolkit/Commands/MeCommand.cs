using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class MeCommand : ICommand
    {
        public string Command => "me";
        public string[] Aliases => new[] { "rpme" };
        public string Description => "РП-действие персонажа. Использование: .me <действие>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "РП-команды выключены."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var action = CommandUtil.Join(arguments).Trim();
            if (string.IsNullOrEmpty(action)) { response = "Использование: .me <действие>"; return false; }

            var name = ResolveName(plugin, p);
            var msg = plugin.Config.RpCommands.MeFormat.Replace("{name}", name).Replace("{action}", action);

            CommandUtil.BroadcastNearby(p, plugin.Config.RpCommands.MeRangeMeters, msg);
            plugin.RoundLog.Append($"ME {p.Nickname}: {action}");

            response = "Отправлено.";
            return true;
        }

        internal static string ResolveName(UstPlugin plugin, LabApi.Features.Wrappers.Player p)
        {
            return plugin.Profiles.TryGet(p.UserId, out var prof) && prof.HasRpName
                ? prof.RpName
                : p.DisplayName;
        }
    }
}
