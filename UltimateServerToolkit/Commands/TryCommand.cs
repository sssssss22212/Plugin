using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class TryCommand : ICommand
    {
        private static readonly Random Rng = new Random();
        private static readonly string[] Outcomes =
        {
            "<color=#9CFF8B>SUCCESS</color>",
            "<color=#FFD27F>PARTIAL SUCCESS</color>",
            "<color=#FF8B8B>FAILURE</color>",
            "<color=#FF5555>CRITICAL FAILURE</color>",
            "<color=#9CFF8B>CRITICAL SUCCESS</color>",
        };

        public string Command => "try";
        public string[] Aliases => new[] { "rptry" };
        public string Description => "Roleplay attempt with a randomized outcome. Usage: .try <action>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "RP commands disabled."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            var action = CommandUtil.Join(arguments).Trim();
            if (string.IsNullOrEmpty(action))
            {
                response = "Usage: .try <action>";
                return false;
            }

            var outcome = Outcomes[Rng.Next(Outcomes.Length)];
            var name = MeCommand.ResolveName(plugin, player);
            var msg = plugin.Config.RpCommands.TryFormat
                .Replace("{name}", name)
                .Replace("{action}", action)
                .Replace("{outcome}", outcome);

            CommandUtil.BroadcastNearby(player, plugin.Config.RpCommands.MeRangeMeters, msg);
            plugin.RoundLog.Append($"TRY {player.Nickname}: {action} -> {outcome}");

            response = "Roll resolved.";
            return true;
        }
    }
}
