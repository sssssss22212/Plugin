using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace UltimateServerToolkit.Modules.Notes
{
    public sealed class DroppedNote
    {
        public string AuthorUserId { get; set; }
        public string AuthorDisplayName { get; set; }
        public string Text { get; set; }
        public Vector3 Position { get; set; }
        public DateTime DroppedAtUtc { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// In-memory, round-scoped registry of "notes" that players have written and dropped.
    /// A note is read by anyone within <see cref="ReadRangeMeters"/> of the drop position.
    /// </summary>
    public sealed class NoteService
    {
        private readonly UstPlugin _plugin;
        private readonly ConcurrentBag<DroppedNote> _notes = new ConcurrentBag<DroppedNote>();

        // Per-author limiter, reset on round.
        private readonly ConcurrentDictionary<string, int> _writesThisRound =
            new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public float ReadRangeMeters { get; set; } = 2.5f;

        public NoteService(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Reset()
        {
            while (_notes.TryTake(out _)) { }
            _writesThisRound.Clear();
        }

        public bool TryDrop(Player author, string text, out string error)
        {
            error = null;
            var cfg = _plugin.Config.Notes;
            if (!cfg.Enabled)
            {
                error = "Notes are disabled.";
                return false;
            }

            text = (text ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                error = "Empty note.";
                return false;
            }
            if (text.Length > cfg.MaxNoteLength)
            {
                error = $"Note too long (max {cfg.MaxNoteLength}).";
                return false;
            }

            var written = _writesThisRound.AddOrUpdate(author.UserId, 1, (_, v) => v + 1);
            if (written > cfg.MaxNotesPerRound)
            {
                error = $"Too many notes this round ({cfg.MaxNotesPerRound}).";
                return false;
            }

            _notes.Add(new DroppedNote
            {
                AuthorUserId = author.UserId,
                AuthorDisplayName = author.DisplayName,
                Text = text,
                Position = author.Position,
            });

            return true;
        }

        public IEnumerable<DroppedNote> GetNearby(Vector3 position)
        {
            var sqr = ReadRangeMeters * ReadRangeMeters;
            foreach (var note in _notes)
            {
                if ((note.Position - position).sqrMagnitude <= sqr)
                    yield return note;
            }
        }
    }
}
