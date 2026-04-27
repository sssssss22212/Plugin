using System;
using System.Collections.Generic;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace UltimateServerToolkit.Modules.AntiRdm
{
    public sealed class AntiRdmService
    {
        private readonly UstPlugin _plugin;

        private readonly Dictionary<string, DateTime> _lastDamagedAt =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public AntiRdmService(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Reset() => _lastDamagedAt.Clear();

        public void OnHurting(Player vic, Player att)
        {
            if (!_plugin.Config.AntiRdm.Enabled || vic == null) return;

            if (att == null || att.UserId == vic.UserId) return;
            if (!IsTeamKill(att, vic)) return;

            var key = $"{att.UserId}|{vic.UserId}";
            if (HasRecentlyAttacked(key))
            {
                _lastDamagedAt[key] = DateTime.UtcNow;
                return;
            }

            _lastDamagedAt[key] = DateTime.UtcNow;

            _plugin.Karma.Penalize(att, _plugin.Config.Karma.RdmPenalty, "Тимдамаг/RDM");

            if (_plugin.Config.AntiRdm.BroadcastWarning)
            {
                try { att.SendHint(_plugin.Config.AntiRdm.RdmWarningHint, duration: 4f); }
                catch { }
            }

            if (_plugin.Profiles.TryGet(att.UserId, out var prof))
            {
                prof.RdmCount++;
                _plugin.ProfileStore.MarkDirty();
            }
        }

        public void OnDeath(Player vic, Player att)
        {
            if (!_plugin.Config.AntiRdm.Enabled) return;
            if (vic == null || att == null || att.UserId == vic.UserId) return;
            if (!IsTeamKill(att, vic)) return;

            _plugin.Karma.Penalize(att, _plugin.Config.Karma.TeamKillPenalty, "Тимкилл");
            Logger.Warn($"[UST/RDM] Тимкилл: {att.Nickname} -> {vic.Nickname}");

            if (_plugin.Profiles.TryGet(att.UserId, out var prof))
            {
                prof.TeamKillCount++;
                _plugin.ProfileStore.MarkDirty();
            }
        }

        private bool HasRecentlyAttacked(string key)
        {
            if (!_lastDamagedAt.TryGetValue(key, out var t)) return false;
            return (DateTime.UtcNow - t).TotalSeconds <= _plugin.Config.AntiRdm.TeamDamageWindowSeconds;
        }

        private static bool IsTeamKill(Player att, Player vic)
        {
            if (att.Team == Team.Dead || vic.Team == Team.Dead) return false;
            return att.Faction == vic.Faction && att.Team != Team.SCPs;
        }
    }
}
