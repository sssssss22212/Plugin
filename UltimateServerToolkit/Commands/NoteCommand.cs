using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class NoteCommand : ICommand
    {
        public string Command => "note";
        public string[] Aliases => new[] { "rpnote" };
        public string Description => "Оставить или прочитать РП-заметки рядом с собой.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.Notes.Enabled) { response = "Заметки выключены."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            if (arguments.Count == 0)
            {
                response = "Использование: .note write <текст>  |  .note read";
                return false;
            }

            var sub = arguments.Array[arguments.Offset].ToLowerInvariant();
            switch (sub)
            {
                case "write":
                case "drop":
                {
                    var text = CommandUtil.Join(arguments, 1).Trim();
                    if (!plugin.Notes.TryDrop(p, text, out var err))
                    {
                        response = err;
                        return false;
                    }
                    plugin.RoundLog.Append($"NOTE_DROP {p.Nickname}: {text}");
                    response = "Заметка оставлена.";
                    return true;
                }
                case "read":
                {
                    var sb = new StringBuilder();
                    var n = 0;
                    foreach (var note in plugin.Notes.GetNearby(p.Position))
                    {
                        sb.AppendLine($"— [{note.AuthorDisplayName}]: {note.Text}");
                        n++;
                    }
                    response = n == 0 ? "Рядом ничего нет." : $"Заметки рядом ({n}):\n{sb}";
                    return true;
                }
                default:
                    response = "Использование: .note write <текст>  |  .note read";
                    return false;
            }
        }
    }
}
