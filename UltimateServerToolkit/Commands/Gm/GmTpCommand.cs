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
        public string Description => "Тп к игроку. Использование: gm tp <игрок>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Использование: gm tp <игрок>"; return false; }

            var q = string.Join(" ", arguments.Array, arguments.Offset, arguments.Count);
            var target = CommandUtil.FindByQuery(q);
            if (target == null) { response = $"Не найден: '{q}'."; return false; }

            var src = Player.Get(sender);
            if (src == null) { response = "tp нельзя из консоли — нужен игрок."; return false; }

            src.Position = target.Position;
            response = $"Тп к {target.DisplayName}.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmBringCommand : ICommand
    {
        public string Command => "bring";
        public string[] Aliases => new[] { "summon" };
        public string Description => "Притянуть игрока к себе. Использование: gm bring <игрок>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Использование: gm bring <игрок>"; return false; }

            var q = string.Join(" ", arguments.Array, arguments.Offset, arguments.Count);
            var target = CommandUtil.FindByQuery(q);
            if (target == null) { response = $"Не найден: '{q}'."; return false; }

            var caller = Player.Get(sender);
            if (caller == null) { response = "bring нужен игрок-вызывающий."; return false; }

            target.Position = caller.Position;
            response = $"Притянул {target.DisplayName}.";
            return true;
        }
    }
}
