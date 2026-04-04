using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace UltimateServerToolkit.Commands
{
    public class GravityCommand : ICommand, IUsageProvider
    {
        public string Command => "ugravity";
        public string[] Aliases => new[] { "ugrav" };
        public string Description => "Change a player's gravity.";
        public string[] Usage => new[] { "player_id", "gravity_y (e.g. -9.81 normal, -2 low, -30 high)" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: ugravity <player_id> <gravity_y>\nExamples: -9.81 (normal), -2 (low/moon), -30 (high), 5 (reverse)";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int playerId))
            {
                response = "Invalid player ID.";
                return false;
            }

            if (!float.TryParse(arguments.At(1), out float gravityY))
            {
                response = "Invalid gravity value.";
                return false;
            }

            Player target = null;
            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.PlayerId == playerId)
                {
                    target = p;
                    break;
                }
            }

            if (target == null)
            {
                response = $"Player {playerId} not found.";
                return false;
            }

            target.Gravity = new Vector3(0f, gravityY, 0f);

            string gravType;
            if (gravityY > 0) gravType = "REVERSE";
            else if (gravityY > -5) gravType = "LOW (Moon)";
            else if (gravityY > -15) gravType = "NORMAL";
            else gravType = "HIGH";

            target.SendHint(
                Utils.HintBuilder.Size(
                    Utils.HintBuilder.Bold(Utils.HintBuilder.Color($"Gravity: {gravType}", "#cc66ff")),
                    20) + "\n" +
                Utils.HintBuilder.Size(
                    Utils.HintBuilder.Color($"Y = {gravityY:F1}", "#aaaaaa"),
                    14),
                3f);

            response = $"Set {target.Nickname} gravity to Y={gravityY:F1} ({gravType}).";
            return true;
        }
    }
}
