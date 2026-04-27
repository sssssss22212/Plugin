using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace UltimateServerToolkit.Commands
{
    internal static class CommandUtil
    {
        public static bool RequirePlayer(ICommandSender sender, out Player p, out string error)
        {
            p = Player.Get(sender);
            if (p != null) { error = null; return true; }
            error = "Команду может вызвать только игрок в игре.";
            return false;
        }

        public static bool RequirePlugin(out UstPlugin plugin, out string error)
        {
            plugin = UstPlugin.Instance;
            if (plugin == null) { error = "Плагин не загружен."; return false; }
            error = null;
            return true;
        }

        private static readonly Regex Placeholder = new Regex(@"\{([a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled);

        public static string Format(string template, params (string key, string value)[] vars)
        {
            if (string.IsNullOrEmpty(template) || vars == null || vars.Length == 0) return template;
            var map = new Dictionary<string, string>(vars.Length, StringComparer.Ordinal);
            foreach (var v in vars) map[v.key] = v.value ?? string.Empty;
            return Placeholder.Replace(template, m => map.TryGetValue(m.Groups[1].Value, out var s) ? s : m.Value);
        }

        public static string Join(ArraySegment<string> args, int startIndex = 0, int? count = null)
        {
            if (args.Count == 0 || startIndex >= args.Count) return string.Empty;
            var take = count ?? (args.Count - startIndex);
            return string.Join(" ", args.Array, args.Offset + startIndex, take);
        }

        public static void BroadcastNearby(Player from, float range, string msg, float duration = 5f)
        {
            if (from == null || string.IsNullOrEmpty(msg)) return;
            var sqr = range * range;
            var pos = from.Position;

            foreach (var p in Player.ReadyList)
            {
                if (p == null || !p.IsPlayer) continue;
                if ((p.Position - pos).sqrMagnitude > sqr) continue;
                try { p.SendHint(msg, duration); }
                catch { }
            }
        }

        public static Player FindNearest(Player from, float range)
        {
            if (from == null) return null;
            Player best = null;
            var bestSqr = range * range;

            foreach (var p in Player.ReadyList)
            {
                if (p == null || !p.IsPlayer || p.UserId == from.UserId) continue;
                var d = (p.Position - from.Position).sqrMagnitude;
                if (d <= bestSqr) { bestSqr = d; best = p; }
            }
            return best;
        }

        public static Player FindByQuery(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return null;

            if (int.TryParse(q, out var id))
                foreach (var p in Player.ReadyList)
                    if (p.PlayerId == id) return p;

            foreach (var p in Player.ReadyList)
                if (string.Equals(p.UserId, q, StringComparison.OrdinalIgnoreCase)) return p;

            var byName = Player.GetByDisplayName(q, requireFullMatch: false);
            if (byName != null) return byName;

            foreach (var p in Player.ReadyList)
                if (p.Nickname != null && p.Nickname.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    return p;

            return null;
        }
    }
}
