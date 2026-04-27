using System;
using System.Collections.Generic;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace UltimateServerToolkit.Modules.AntiRdm
{
    /// <summary>
    /// Detects same-team damage/kills with no preceding aggression and applies karma penalties.
    /// State resets on each round.
    /// </summary>
    public sealed class AntiRdmService
    {
        private readonly UstPlugin _plugin;

        // Round-scoped: when did we last see Player X being damaged by *anyone* (any team).
        private readonly Dictionary<string, DateTime> _lastDamagedAt =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public AntiRdmService(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Reset() => _lastDamagedAt.Clear();

        /// <summary>Called from <see cref="EventsHandler"/> on every Hurting event.</summary>
        public void OnHurting(Player victim, Player attacker)
        {
            if (!_plugin.Config.AntiRdm.Enabled) return;
            if (victim == null) return;

            _lastDamagedAt[victim.UserId] = DateTime.UtcNow;

            if (attacker == null || attacker.UserId == victim.UserId) return;
            if (!IsTeamKill(attacker, victim)) return;

            // RDM = team-attack and victim has not damaged anyone recently (no provocation).
            if (HasRecentlyAttacked(victim)) return;

            _plugin.Karma.Penalize(attacker, _plugin.Config.Karma.RdmPenalty, "Team-damage RDM");

            if (_plugin.Config.AntiRdm.BroadcastWarning)
            {
                try
                {
                    attacker.SendHint(_plugin.Config.AntiRdm.RdmWarningHint, duration: 4f);
                }
                catch
                {
                    // ignore
                }
            }

            if (_plugin.Profiles.TryGet(attacker.UserId, out var profile))
            {
                profile.RdmCount++;
                _plugin.ProfileStore.MarkDirty();
            }
        }

        /// <summary>Called from <see cref="EventsHandler"/> on every Death event.</summary>
        public void OnDeath(Player victim, Player attacker)
        {
            if (!_plugin.Config.AntiRdm.Enabled) return;
            if (victim == null || attacker == null) return;
            if (attacker.UserId == victim.UserId) return;
            if (!IsTeamKill(attacker, victim)) return;

            _plugin.Karma.Penalize(attacker, _plugin.Config.Karma.TeamKillPenalty, "Team kill");
            Logger.Warn($"[UST/AntiRDM] Team kill: {attacker.Nickname} -> {victim.Nickname}");

            if (_plugin.Profiles.TryGet(attacker.UserId, out var profile))
            {
                profile.TeamKillCount++;
                _plugin.ProfileStore.MarkDirty();
            }
        }

        private bool HasRecentlyAttacked(Player victim)
        {
            if (!_lastDamagedAt.TryGetValue(victim.UserId, out var when)) return false;
            return (DateTime.UtcNow - when).TotalSeconds <=
                   _plugin.Config.AntiRdm.TeamDamageWindowSeconds;
        }

        private static bool IsTeamKill(Player attacker, Player victim)
        {
            if (attacker.Team == Team.Dead || victim.Team == Team.Dead) return false;

            return attacker.Faction == victim.Faction
                   && attacker.Team != Team.SCPs; // SCPs are solo, treat normally.
        }
    }
}
