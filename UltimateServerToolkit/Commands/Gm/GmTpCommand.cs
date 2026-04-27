using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Commands.Gm
{
    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmTpCommand : ICommand
    {
        public string Command => "tp";
        public string[] Aliases => new[] { "teleport" };
        public string Description => "Teleport to a player. Usage: gm tp <player-id|name>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Usage: gm tp <player>"; return false; }

            var query = string.Join(" ", arguments.Array, arguments.Offset, arguments.Count);
            var target = CommandUtil.FindByQuery(query);
            if (target == null) { response = $"No player matched '{query}'."; return false; }

            var src = Player.Get(sender);
            if (src == null)
            {
                response = "tp requires a player executor (cannot run from server console).";
                return false;
            }

            src.Position = target.Position;
            response = $"Teleported to {target.DisplayName}.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmBringCommand : ICommand
    {
        public string Command => "bring";
        public string[] Aliases => new[] { "summon" };
        public string Description => "Teleport another player to you. Usage: gm bring <player>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Usage: gm bring <player>"; return false; }

            var query = string.Join(" ", arguments.Array, arguments.Offset, arguments.Count);
            var target = CommandUtil.FindByQuery(query);
            if (target == null) { response = $"No player matched '{query}'."; return false; }

            var caller = Player.Get(sender);
            if (caller == null)
            {
                response = "bring requires a player executor.";
                return false;
            }

            target.Position = caller.Position;
            response = $"Brought {target.DisplayName} to you.";
            return true;
        }
    }
}
