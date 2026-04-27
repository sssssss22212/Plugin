using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;


namespace UltimateServerToolkit.Commands
{
    internal static class CommandUtil
    {
        public static bool RequirePlayer(ICommandSender sender, out Player player, out string error)
        {
            player = Player.Get(sender);
            if (player != null) { error = null; return true; }
            error = "This command can only be used by an in-game player.";
            return false;
        }

        public static bool RequirePlugin(out UstPlugin plugin, out string error)
        {
            plugin = UstPlugin.Instance;
            if (plugin == null)
            {
                error = "UltimateServerToolkit is not loaded.";
                return false;
            }
            error = null;
            return true;
        }

        public static string Join(ArraySegment<string> args, int startIndex = 0, int? count = null)
        {
            if (args.Count == 0 || startIndex >= args.Count) return string.Empty;
            var take = count ?? (args.Count - startIndex);
            return string.Join(" ", args.Array, args.Offset + startIndex, take);
        }

        public static void BroadcastNearby(Player from, float rangeMeters, string message, float duration = 5f)
        {
            if (from == null || string.IsNullOrEmpty(message)) return;
            var sqr = rangeMeters * rangeMeters;
            var origin = from.Position;

            foreach (var other in Player.ReadyList)
            {
                if (other == null || !other.IsPlayer) continue;
                if ((other.Position - origin).sqrMagnitude > sqr) continue;

                try
                {
                    other.SendHint(message, duration);
                }
                catch
                {
                    // ignore
                }
            }
        }

        public static Player FindNearest(Player from, float maxRangeMeters)
        {
            if (from == null) return null;
            Player best = null;
            var bestSqr = maxRangeMeters * maxRangeMeters;

            foreach (var other in Player.ReadyList)
            {
                if (other == null || !other.IsPlayer || other.UserId == from.UserId) continue;
                var d = (other.Position - from.Position).sqrMagnitude;
                if (d <= bestSqr)
                {
                    bestSqr = d;
                    best = other;
                }
            }
            return best;
        }

        public static Player FindByQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            // Try playerId, UserId, then DisplayName/Nickname.
            if (int.TryParse(query, out var id))
            {
                foreach (var p in Player.ReadyList)
                    if (p.PlayerId == id) return p;
            }

            foreach (var p in Player.ReadyList)
            {
                if (string.Equals(p.UserId, query, StringComparison.OrdinalIgnoreCase)) return p;
            }

            var byDisplay = Player.GetByDisplayName(query, requireFullMatch: false);
            if (byDisplay != null) return byDisplay;

            foreach (var p in Player.ReadyList)
            {
                if (p.Nickname != null && p.Nickname.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    return p;
            }

            return null;
        }
    }
}
