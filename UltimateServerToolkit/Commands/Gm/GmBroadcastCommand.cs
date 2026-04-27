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
        public string Description => "Объявление всем. Использование: gm broadcast <текст>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Использование: gm broadcast <текст>"; return false; }

            var msg = string.Join(" ", arguments.Array, arguments.Offset, arguments.Count);
            var line = $"<b><color=#ffd27f>[GM]</color></b> {msg}";

            foreach (var p in Player.ReadyList)
            {
                try { p.SendBroadcast(line, 8, Broadcast.BroadcastFlags.Normal, true); } catch { }
            }

            UstPlugin.Instance?.RoundLog?.Append($"GM_BROADCAST: {msg}");
            response = "Объявление отправлено.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmEventCommand : ICommand
    {
        public string Command => "event";
        public string[] Aliases => new[] { "rpevent" };
        public string Description => "Объявить РП-событие. Использование: gm event <текст>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Использование: gm event <текст>"; return false; }

            var msg = string.Join(" ", arguments.Array, arguments.Offset, arguments.Count);
            var hint =
                "<size=28><b><color=#ff5555>** GM-СОБЫТИЕ **</color></b></size>\n" +
                "<size=22><i>" + msg + "</i></size>";

            foreach (var p in Player.ReadyList)
            {
                try { p.SendHint(hint, 8f); } catch { }
            }

            UstPlugin.Instance?.RoundLog?.Append($"GM_EVENT: {msg}");
            response = "Событие объявлено.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmKarmaCommand : ICommand
    {
        public string Command => "karma";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Изменить карму игрока. Использование: gm karma <игрок> <±N> [причина]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count < 2) { response = "Использование: gm karma <игрок> <±N> [причина]"; return false; }

            var target = CommandUtil.FindByQuery(arguments.Array[arguments.Offset]);
            if (target == null) { response = "Игрок не найден."; return false; }

            if (!int.TryParse(arguments.Array[arguments.Offset + 1], out var delta))
            {
                response = "Сумма должна быть целым числом.";
                return false;
            }

            var reason = arguments.Count > 2
                ? string.Join(" ", arguments.Array, arguments.Offset + 2, arguments.Count - 2)
                : "GM-правка";

            var plugin = UstPlugin.Instance;
            if (plugin == null) { response = "Плагин не инициализирован."; return false; }

            if (delta < 0) plugin.Karma.Penalize(target, -delta, reason);
            else if (delta > 0) plugin.Karma.Reward(target, delta, reason);
            else { response = "Сумма должна быть ненулевой."; return false; }

            response = $"Карма {target.DisplayName} изменена на {delta} ({reason}).";
            return true;
        }
    }
}
