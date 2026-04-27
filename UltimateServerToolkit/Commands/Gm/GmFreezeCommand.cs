using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace UltimateServerToolkit.Commands.Gm
{
    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmFreezeCommand : ICommand
    {
        private static readonly Dictionary<string, Vector3> Frozen = new Dictionary<string, Vector3>();

        public string Command => "freeze";
        public string[] Aliases => new[] { "f" };
        public string Description => "Заморозить/разморозить игрока. Использование: gm freeze <игрок>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Использование: gm freeze <игрок>"; return false; }

            var target = CommandUtil.FindByQuery(string.Join(" ", arguments.Array, arguments.Offset, arguments.Count));
            if (target == null) { response = "Игрок не найден."; return false; }

            if (Frozen.Remove(target.UserId))
            {
                response = $"Разморозил {target.DisplayName}.";
                return true;
            }

            Frozen[target.UserId] = target.Position;
            MEC.Timing.RunCoroutine(FreezeLoop(target.UserId));
            response = $"Заморозил {target.DisplayName}.";
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
