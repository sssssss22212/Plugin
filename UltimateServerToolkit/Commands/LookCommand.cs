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
        public string Description => "Осмотреть ближайшего игрока и прочитать его био.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.RpCommands.Enabled) { response = "РП-команды выключены."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var target = CommandUtil.FindNearest(p, plugin.Config.RpCommands.LookRangeMeters);
            if (target == null) { response = "Никого не видно."; return false; }

            var name = MeCommand.ResolveName(plugin, target);
            var bio = plugin.Profiles.TryGet(target.UserId, out var prof) && prof.HasBio
                ? prof.Bio
                : "<i>Ничего особенного.</i>";

            var msg = plugin.Config.RpCommands.LookFormat.Replace("{target}", name).Replace("{bio}", bio);

            try { p.SendHint(msg, 6f); } catch { }
            plugin.RoundLog.Append($"LOOK {p.Nickname} -> {target.Nickname}");

            response = "OK";
            return true;
        }
    }
}
