using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace UltimateServerToolkit.Modules.Whitelist
{
    public sealed class WhitelistService
    {
        private readonly UstPlugin _plugin;
        private HashSet<RoleTypeId> _restrictedCache;
        private string[] _lastRestricted;

        public WhitelistService(UstPlugin plugin)
        {
            _plugin = plugin;
        }

        public bool IsAllowed(Player player, RoleTypeId newRole)
        {
            var cfg = _plugin.Config.Whitelist;
            if (!cfg.Enabled) return true;

            EnsureCacheFresh(cfg.RestrictedRoles);
            if (!_restrictedCache.Contains(newRole)) return true;

            return cfg.AllowedUserIds.Contains(player.UserId, StringComparer.OrdinalIgnoreCase);
        }

        private void EnsureCacheFresh(IList<string> source)
        {
            if (_lastRestricted != null &&
                _lastRestricted.Length == source.Count &&
                !_lastRestricted.Where((t, i) => !string.Equals(t, source[i], StringComparison.OrdinalIgnoreCase)).Any())
            {
                return;
            }

            _restrictedCache = new HashSet<RoleTypeId>();
            foreach (var name in source)
            {
                if (Enum.TryParse(name, ignoreCase: true, result: out RoleTypeId role))
                    _restrictedCache.Add(role);
            }
            _lastRestricted = source.ToArray();
        }
    }
}
