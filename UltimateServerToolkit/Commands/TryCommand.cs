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
            "<color=#9CFF8B>УСПЕХ</color>",
            "<color=#FFD27F>ЧАСТИЧНО</color>",
            "<color=#FF8B8B>ПРОВАЛ</color>",
            "<color=#FF5555>КРИТ.ПРОВАЛ</color>",
            "<color=#9CFF8B>КРИТ.УСПЕХ</color>",
        };

        public string Command => "try";
        public string[] Aliases => new[] { "rptry" };
        public string Description => "Попытка с случайным исходом. Использование: .try <действие>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "РП-команды выключены."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var action = CommandUtil.Join(arguments).Trim();
            if (string.IsNullOrEmpty(action)) { response = "Использование: .try <действие>"; return false; }

            var outcome = Outcomes[Rng.Next(Outcomes.Length)];
            var name = MeCommand.ResolveName(plugin, p);
            var msg = plugin.Config.RpCommands.TryFormat
                .Replace("{name}", name)
                .Replace("{action}", action)
                .Replace("{outcome}", outcome);

            CommandUtil.BroadcastNearby(p, plugin.Config.RpCommands.MeRangeMeters, msg);
            plugin.RoundLog.Append($"TRY {p.Nickname}: {action} -> {outcome}");

            response = "Бросок сделан.";
            return true;
        }
    }
}
