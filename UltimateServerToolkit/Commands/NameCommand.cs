using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class NameCommand : ICommand
    {
        public string Command => "rpname";
        public string[] Aliases => new[] { "rname", "name" };
        public string Description => "Set your RP character name. Usage: .rpname <name>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var newName = CommandUtil.Join(arguments).Trim();
            if (!plugin.Profiles.TrySetName(player, newName, out var error))
            {
                response = error ?? "Failed to set name.";
                return false;
            }

            response = $"RP name set to: {newName}";
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class BioCommand : ICommand
    {
        public string Command => "bio";
        public string[] Aliases => new[] { "rpbio" };
        public string Description => "Set your RP bio shown when others use .look. Usage: .bio <text>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var text = CommandUtil.Join(arguments).Trim();
            if (!plugin.Profiles.TrySetBio(player, text, out var error))
            {
                response = error ?? "Failed to set bio.";
                return false;
            }

            response = string.IsNullOrEmpty(text) ? "Bio cleared." : "Bio updated.";
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class FactionCommand : ICommand
    {
        public string Command => "rpfaction";
        public string[] Aliases => new[] { "faction" };
        public string Description => "Set your RP faction/affiliation tag. Usage: .rpfaction <text>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var text = CommandUtil.Join(arguments).Trim();
            if (!plugin.Profiles.TrySetFaction(player, text, out var error))
            {
                response = error ?? "Failed to set faction.";
                return false;
            }

            response = string.IsNullOrEmpty(text) ? "Faction cleared." : $"Faction set: {text}";
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class KarmaCommand : ICommand
    {
        public string Command => "karma";
        public string[] Aliases => new[] { "rpkarma" };
        public string Description => "Show your current karma and RP record.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var profile = plugin.Profiles.GetOrCreate(player);
            response =
                $"Karma: {profile.Karma}\n" +
                $"RDM count: {profile.RdmCount}\n" +
                $"Team kills: {profile.TeamKillCount}\n" +
                $"RP name: {(profile.HasRpName ? profile.RpName : "(unset)")}\n" +
                $"First seen: {profile.FirstSeenUtc:yyyy-MM-dd}";
            return true;
        }
    }
}
