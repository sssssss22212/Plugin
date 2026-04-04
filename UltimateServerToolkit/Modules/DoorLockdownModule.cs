using System.Collections.Generic;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Modules
{
    public class DoorLockdownModule
    {
        private readonly DoorLockdownConfig _config;
        private bool _lockdownActive;

        public DoorLockdownModule(DoorLockdownConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            Logger.Info("[DoorLockdown] Module enabled.");
        }

        public void Disable()
        {
            Logger.Info("[DoorLockdown] Module disabled.");
        }

        public void StartLockdown(FacilityZone zone)
        {
            if (_lockdownActive)
            {
                Logger.Warn("[DoorLockdown] Lockdown already active.");
                return;
            }

            _lockdownActive = true;
            Timing.RunCoroutine(LockdownCoroutine(zone));
        }

        public void StartFullLockdown()
        {
            if (_lockdownActive)
            {
                Logger.Warn("[DoorLockdown] Lockdown already active.");
                return;
            }

            _lockdownActive = true;
            Timing.RunCoroutine(FullLockdownCoroutine());
        }

        private IEnumerator<float> LockdownCoroutine(FacilityZone zone)
        {
            string zoneName = zone.ToString();
            Logger.Info($"[DoorLockdown] Starting lockdown in zone: {zoneName}");

            // Lock all doors in zone
            var lockedDoors = new List<Door>();
            foreach (var door in Map.Doors)
            {
                if (door == null) continue;

                bool inZone = false;
                foreach (var room in door.Rooms)
                {
                    if (room != null && room.Zone == zone)
                    {
                        inZone = true;
                        break;
                    }
                }

                if (inZone)
                {
                    door.IsOpened = false;
                    door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.AdminCommand, true);
                    lockedDoors.Add(door);
                }
            }

            // Flicker lights in zone
            foreach (var light in Map.RoomLights)
            {
                if (light != null && light.Room != null && light.Room.Zone == zone)
                {
                    light.FlickerLights(2f);
                }
            }

            // Announce
            string warning = HintBuilder.WarningBanner(
                $"ZONE LOCKDOWN: {zoneName}",
                $"All doors locked for {_config.LockdownDurationSeconds}s!");

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(warning, 5f);
            }

            Announcer.Message($"Attention . Zone lockdown initiated in {zoneName} . All doors have been sealed", "", true, 0f, 1f);

            yield return Timing.WaitForSeconds(_config.LockdownDurationSeconds);

            // Unlock all doors
            foreach (var door in lockedDoors)
            {
                if (door != null)
                    door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.AdminCommand, false);
            }

            _lockdownActive = false;

            string endMsg = HintBuilder.Size(
                HintBuilder.Bold(HintBuilder.Color($"Zone lockdown lifted: {zoneName}", "#00ff00")),
                22);

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(endMsg, 4f);
            }

            Announcer.Message($"Zone lockdown in {zoneName} has been lifted", "", true, 0f, 1f);
            Logger.Info($"[DoorLockdown] Lockdown ended in zone: {zoneName}");
        }

        private IEnumerator<float> FullLockdownCoroutine()
        {
            Logger.Info("[DoorLockdown] Starting FULL FACILITY lockdown!");

            var lockedDoors = new List<Door>();
            foreach (var door in Map.Doors)
            {
                if (door == null) continue;
                door.IsOpened = false;
                door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.AdminCommand, true);
                lockedDoors.Add(door);
            }

            foreach (var light in Map.RoomLights)
            {
                if (light != null)
                    light.FlickerLights(3f);
            }

            string warning = HintBuilder.WarningBanner(
                "FULL FACILITY LOCKDOWN",
                $"ALL doors locked for {_config.LockdownDurationSeconds}s!");

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(warning, 5f);
            }

            Announcer.Message("Attention . Full facility lockdown initiated . All doors have been sealed", "", true, 0f, 1f);

            yield return Timing.WaitForSeconds(_config.LockdownDurationSeconds);

            foreach (var door in lockedDoors)
            {
                if (door != null)
                    door.Lock(Interactables.Interobjects.DoorUtils.DoorLockReason.AdminCommand, false);
            }

            _lockdownActive = false;

            string endMsg = HintBuilder.Size(
                HintBuilder.Bold(HintBuilder.Color("Full facility lockdown lifted", "#00ff00")),
                22);

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.IsReady && !p.IsHost)
                    p.SendHint(endMsg, 4f);
            }

            Announcer.Message("Full facility lockdown has been lifted . All doors are now operational", "", true, 0f, 1f);
            Logger.Info("[DoorLockdown] Full lockdown ended.");
        }

        public bool IsLockdownActive => _lockdownActive;
    }
}
