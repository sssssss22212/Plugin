using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace UltimateServerToolkit.Commands
{
    public class SizeCommand : ICommand, IUsageProvider
    {
        public string Command => "usize";
        public string[] Aliases => new[] { "us", "scale" };
        public string Description => "Change a player's size/scale.";
        public string[] Usage => new[] { "player_id", "x", "y", "z" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: usize <player_id> <scale> OR usize <player_id> <x> <y> <z>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int playerId))
            {
                response = "Invalid player ID.";
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

            Vector3 scale;

            if (arguments.Count >= 4)
            {
                if (!float.TryParse(arguments.At(1), out float x) ||
                    !float.TryParse(arguments.At(2), out float y) ||
                    !float.TryParse(arguments.At(3), out float z))
                {
                    response = "Invalid scale values.";
                    return false;
                }
                scale = new Vector3(x, y, z);
            }
            else
            {
                if (!float.TryParse(arguments.At(1), out float s))
                {
                    response = "Invalid scale value.";
                    return false;
                }
                scale = new Vector3(s, s, s);
            }

            target.Scale = scale;

            target.SendHint(
                Utils.HintBuilder.Size(
                    Utils.HintBuilder.Color($"Size changed to {scale.x:F1}x{scale.y:F1}x{scale.z:F1}", "#ff66ff"),
                    18),
                3f);

            response = $"Set {target.Nickname} scale to ({scale.x:F1}, {scale.y:F1}, {scale.z:F1}).";
            return true;
        }
    }
}
