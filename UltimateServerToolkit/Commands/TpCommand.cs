using System;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Commands
{
    public class TpCommand : ICommand, IUsageProvider
    {
        public string Command => "utp";
        public string[] Aliases => new[] { "uteleport" };
        public string Description => "Teleport a player to another player.";
        public string[] Usage => new[] { "source_id", "target_id" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: utp <source_id> <target_id>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int sourceId) || !int.TryParse(arguments.At(1), out int targetId))
            {
                response = "Invalid player IDs.";
                return false;
            }

            Player source = null;
            Player target = null;

            foreach (var p in Player.ReadyList)
            {
                if (p == null) continue;
                if (p.PlayerId == sourceId) source = p;
                if (p.PlayerId == targetId) target = p;
            }

            if (source == null)
            {
                response = $"Source player {sourceId} not found.";
                return false;
            }

            if (target == null)
            {
                response = $"Target player {targetId} not found.";
                return false;
            }

            if (!source.IsAlive)
            {
                response = $"{source.Nickname} is not alive.";
                return false;
            }

            if (!target.IsAlive)
            {
                response = $"{target.Nickname} is not alive.";
                return false;
            }

            source.Position = target.Position;

            source.SendHint(
                Utils.HintBuilder.Size(
                    Utils.HintBuilder.Color($"Teleported to {target.Nickname}", "#00ffcc"),
                    18),
                2f);

            response = $"Teleported {source.Nickname} to {target.Nickname}.";
            return true;
        }
    }
}
