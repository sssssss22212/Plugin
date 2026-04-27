using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace UltimateServerToolkit.Commands.Gm
{
    /// <summary>
    /// Freeze a player by repeatedly snapping them back to their starting position.
    /// Implemented via a coroutine loop on the player's ReferenceHub.
    /// </summary>
    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmFreezeCommand : ICommand
    {
        // userId -> snapshot of their position at the time of freeze
        private static readonly Dictionary<string, Vector3> Frozen = new Dictionary<string, Vector3>();

        public string Command => "freeze";
        public string[] Aliases => new[] { "f" };
        public string Description => "Toggle a freeze on a player (snaps them in place). Usage: gm freeze <player>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Usage: gm freeze <player>"; return false; }

            var target = CommandUtil.FindByQuery(string.Join(" ", arguments.Array, arguments.Offset, arguments.Count));
            if (target == null) { response = "Target not found."; return false; }

            if (Frozen.Remove(target.UserId))
            {
                response = $"Unfroze {target.DisplayName}.";
                return true;
            }

            Frozen[target.UserId] = target.Position;
            MEC.Timing.RunCoroutine(FreezeLoop(target.UserId));
            response = $"Froze {target.DisplayName}.";
            return true;
        }

        private static IEnumerator<float> FreezeLoop(string userId)
        {
            while (Frozen.TryGetValue(userId, out var anchor))
            {
                Player target = null;
                foreach (var p in Player.ReadyList)
                {
                    if (string.Equals(p.UserId, userId, StringComparison.OrdinalIgnoreCase))
                    {
                        target = p;
                        break;
                    }
                }
                if (target == null) yield break;
                target.Position = anchor;
                yield return MEC.Timing.WaitForSeconds(0.1f);
            }
        }
    }
}
