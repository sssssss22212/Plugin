using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class LookCommand : ICommand
    {
        public string Command => "look";
        public string[] Aliases => new[] { "rplook", "examine" };
        public string Description => "Examine the nearest player and read their RP bio.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "RP commands disabled."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var target = CommandUtil.FindNearest(player, plugin.Config.RpCommands.LookRangeMeters);
            if (target == null)
            {
                response = "No one within sight.";
                return false;
            }

            var name = MeCommand.ResolveName(plugin, target);
            var bio = plugin.Profiles.TryGet(target.UserId, out var profile) && profile.HasBio
                ? profile.Bio
                : "<i>No notable details.</i>";

            var msg = plugin.Config.RpCommands.LookFormat
                .Replace("{target}", name)
                .Replace("{bio}", bio);

            try { player.SendHint(msg, 6f); } catch { /* ignore */ }
            plugin.RoundLog.Append($"LOOK {player.Nickname} -> {target.Nickname}");

            response = "OK";
            return true;
        }
    }
}
