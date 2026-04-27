using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    /// <summary>
    /// .note write &lt;text&gt; — drop a note at your current position.<br/>
    /// .note read       — read all nearby notes.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class NoteCommand : ICommand
    {
        public string Command => "note";
        public string[] Aliases => new[] { "rpnote" };
        public string Description => "Drop or read RP notes at your current location.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!plugin.Config.Notes.Enabled) { response = "Notes disabled."; return false; }
            if (!CommandUtil.RequirePlayer(sender, out var player, out response)) return false;

            if (arguments.Count == 0)
            {
                response = "Usage: .note write <text>  |  .note read";
                return false;
            }

            var sub = arguments.Array[arguments.Offset].ToLowerInvariant();
            switch (sub)
            {
                case "write":
                case "drop":
                {
                    var text = CommandUtil.Join(arguments, 1).Trim();
                    if (!plugin.Notes.TryDrop(player, text, out var err))
                    {
                        response = err;
                        return false;
                    }
                    plugin.RoundLog.Append($"NOTE_DROP {player.Nickname}: {text}");
                    response = "Note dropped.";
                    return true;
                }
                case "read":
                {
                    var sb = new StringBuilder();
                    var count = 0;
                    foreach (var note in plugin.Notes.GetNearby(player.Position))
                    {
                        sb.AppendLine($"— [{note.AuthorDisplayName}]: {note.Text}");
                        count++;
                    }
                    response = count == 0
                        ? "No notes nearby."
                        : $"Notes nearby ({count}):\n{sb}";
                    return true;
                }
                default:
                    response = "Usage: .note write <text>  |  .note read";
                    return false;
            }
        }
    }
}
