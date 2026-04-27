using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Commands.Gm
{
    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmBroadcastCommand : ICommand
    {
        public string Command => "broadcast";
        public string[] Aliases => new[] { "bc" };
        public string Description => "Broadcast a server-wide RP message. Usage: gm broadcast <message>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Usage: gm broadcast <message>"; return false; }

            var msg = string.Join(" ", arguments.Array, arguments.Offset, arguments.Count);
            var formatted = $"<b><color=#ffd27f>[GM]</color></b> {msg}";

            foreach (var p in Player.ReadyList)
            {
                try { p.SendBroadcast(formatted, 8, Broadcast.BroadcastFlags.Normal, true); } catch { }
            }

            UstPlugin.Instance?.RoundLog?.Append($"GM_BROADCAST: {msg}");
            response = "Broadcast sent.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmEventCommand : ICommand
    {
        public string Command => "event";
        public string[] Aliases => new[] { "rpevent" };
        public string Description => "Stage a Game-Master event hint to all players. Usage: gm event <text>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Usage: gm event <text>"; return false; }

            var msg = string.Join(" ", arguments.Array, arguments.Offset, arguments.Count);
            var hint =
                "<size=28><b><color=#ff5555>** GM EVENT **</color></b></size>\n" +
                "<size=22><i>" + msg + "</i></size>";

            foreach (var p in Player.ReadyList)
            {
                try { p.SendHint(hint, 8f); } catch { }
            }

            UstPlugin.Instance?.RoundLog?.Append($"GM_EVENT: {msg}");
            response = "Event posted.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmKarmaCommand : ICommand
    {
        public string Command => "karma";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Adjust a player's karma. Usage: gm karma <player> <±amount> [reason...]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count < 2) { response = "Usage: gm karma <player> <±amount> [reason]"; return false; }

            var target = CommandUtil.FindByQuery(arguments.Array[arguments.Offset]);
            if (target == null) { response = "Target not found."; return false; }

            if (!int.TryParse(arguments.Array[arguments.Offset + 1], out var delta))
            {
                response = "Amount must be an integer.";
                return false;
            }

            var reason = arguments.Count > 2
                ? string.Join(" ", arguments.Array, arguments.Offset + 2, arguments.Count - 2)
                : "GM adjustment";

            var plugin = UstPlugin.Instance;
            if (plugin == null) { response = "Plugin not initialized."; return false; }

            if (delta < 0) plugin.Karma.Penalize(target, -delta, reason);
            else plugin.Karma.Reward(target, delta, reason);

            response = $"Adjusted {target.DisplayName}'s karma by {delta} ({reason}).";
            return true;
        }
    }
}
