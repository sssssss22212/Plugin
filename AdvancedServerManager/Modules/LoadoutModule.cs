using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace AdvancedServerManager.Modules
{
    /// <summary>
    /// Custom Spawn Loadout module that gives players custom items based on their role.
    /// Configured via YAML — each role can have a list of items to receive on spawn.
    /// </summary>
    public class LoadoutModule
    {
        private readonly LoadoutConfig _config;
        private readonly Dictionary<RoleTypeId, List<ItemType>> _parsedLoadouts = new Dictionary<RoleTypeId, List<ItemType>>();

        public LoadoutModule(LoadoutConfig config)
        {
            _config = config;
            ParseLoadouts();
        }

        public void Enable()
        {
            PlayerEvents.Spawned += OnPlayerSpawned;
            LabApi.Features.Console.Logger.Info($"[Loadouts] Module enabled with {_parsedLoadouts.Count} custom loadout(s).");
        }

        public void Disable()
        {
            PlayerEvents.Spawned -= OnPlayerSpawned;
            LabApi.Features.Console.Logger.Info("[Loadouts] Module disabled.");
        }

        private void ParseLoadouts()
        {
            _parsedLoadouts.Clear();

            if (_config.RoleLoadouts == null) return;

            foreach (var kvp in _config.RoleLoadouts)
            {
                if (!Enum.TryParse<RoleTypeId>(kvp.Key, true, out var role))
                {
                    LabApi.Features.Console.Logger.Warn($"[Loadouts] Unknown role '{kvp.Key}' in config — skipping.");
                    continue;
                }

                var items = new List<ItemType>();
                foreach (var itemName in kvp.Value)
                {
                    if (Enum.TryParse<ItemType>(itemName, true, out var itemType))
                    {
                        items.Add(itemType);
                    }
                    else
                    {
                        LabApi.Features.Console.Logger.Warn($"[Loadouts] Unknown item '{itemName}' for role '{kvp.Key}' — skipping.");
                    }
                }

                if (items.Count > 0)
                {
                    _parsedLoadouts[role] = items;
                    LabApi.Features.Console.Logger.Info($"[Loadouts] Registered {items.Count} custom item(s) for role {role}.");
                }
            }
        }

        private void OnPlayerSpawned(PlayerSpawnedEventArgs args)
        {
            if (args.Player == null) return;

            RoleTypeId role = args.Player.Role;

            if (!_parsedLoadouts.TryGetValue(role, out var loadout)) return;

            foreach (var item in loadout)
            {
                args.Player.AddItem(item);
            }

            LabApi.Features.Console.Logger.Debug($"[Loadouts] Gave {loadout.Count} custom item(s) to {args.Player.Nickname} ({role}).");
        }
    }
}
