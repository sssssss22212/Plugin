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
        public string Description => "Roleplay action describing your character. Usage: .me <action>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "RP commands disabled."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var action = CommandUtil.Join(arguments).Trim();
            if (string.IsNullOrEmpty(action))
            {
                response = "Usage: .me <action>";
                return false;
            }

            var name = ResolveName(plugin, player);
            var msg = plugin.Config.RpCommands.MeFormat
                .Replace("{name}", name)
                .Replace("{action}", action);

            CommandUtil.BroadcastNearby(player, plugin.Config.RpCommands.MeRangeMeters, msg);
            plugin.RoundLog.Append($"ME {player.Nickname}: {action}");

            response = "Action sent.";
            return true;
        }

        internal static string ResolveName(UstPlugin plugin, LabApi.Features.Wrappers.Player player)
        {
            return plugin.Profiles.TryGet(player.UserId, out var profile) && profile.HasRpName
                ? profile.RpName
                : player.DisplayName;
        }
    }
}
