using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using MEC;
using UnityEngine;

namespace AdvancedServerManager.Modules
{
    /// <summary>
    /// Anti-AFK module that tracks player movement and warns/moves/kicks idle players.
    /// Uses a coroutine to periodically check all alive players for inactivity.
    /// </summary>
    public class AntiAfkModule
    {
        private readonly AntiAfkConfig _config;
        private readonly Dictionary<string, PlayerAfkData> _playerData = new Dictionary<string, PlayerAfkData>();
        private CoroutineHandle _checkCoroutine;

        public AntiAfkModule(AntiAfkConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            PlayerEvents.Joined += OnPlayerJoined;
            PlayerEvents.Left += OnPlayerLeft;
            PlayerEvents.ChangedRole += OnPlayerChangedRole;
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;

            LabApi.Features.Console.Logger.Info("[AntiAFK] Module enabled.");
        }

        public void Disable()
        {
            PlayerEvents.Joined -= OnPlayerJoined;
            PlayerEvents.Left -= OnPlayerLeft;
            PlayerEvents.ChangedRole -= OnPlayerChangedRole;
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;

            if (_checkCoroutine.IsRunning)
                Timing.KillCoroutines(_checkCoroutine);

            _playerData.Clear();

            LabApi.Features.Console.Logger.Info("[AntiAFK] Module disabled.");
        }

        private void OnRoundStarted()
        {
            _playerData.Clear();
            _checkCoroutine = Timing.RunCoroutine(AfkCheckLoop());
        }

        private void OnRoundEnded(RoundEndedEventArgs args)
        {
            if (_checkCoroutine.IsRunning)
                Timing.KillCoroutines(_checkCoroutine);

            _playerData.Clear();
        }

        private void OnPlayerJoined(PlayerJoinedEventArgs args)
        {
            if (args.Player == null) return;
            string odString = args.Player.UserId;
            if (string.IsNullOrEmpty(odString)) return;

            _playerData[odString] = new PlayerAfkData
            {
                LastPosition = args.Player.Position,
                LastActiveTime = Time.time,
                WarningSent = false
            };
        }

        private void OnPlayerLeft(PlayerLeftEventArgs args)
        {
            if (args.Player == null) return;
            string odString = args.Player.UserId;
            if (!string.IsNullOrEmpty(odString))
                _playerData.Remove(odString);
        }

        private void OnPlayerChangedRole(PlayerChangedRoleEventArgs args)
        {
            if (args.Player == null) return;
            string odString = args.Player.UserId;
            if (string.IsNullOrEmpty(odString)) return;

            // Reset AFK timer on role change
            if (_playerData.ContainsKey(odString))
            {
                _playerData[odString].LastPosition = args.Player.Position;
                _playerData[odString].LastActiveTime = Time.time;
                _playerData[odString].WarningSent = false;
            }
        }

        private IEnumerator<float> AfkCheckLoop()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(5f);

                foreach (var player in Player.ReadyList)
                {
                    if (player == null) continue;

                    string odString = player.UserId;
                    if (string.IsNullOrEmpty(odString)) continue;

                    // Skip spectators if configured
                    if (_config.IgnoreSpectators && player.Role == RoleTypeId.Spectator)
                        continue;

                    // Skip non-alive players
                    if (!player.IsAlive) continue;

                    if (!_playerData.TryGetValue(odString, out var data))
                    {
                        _playerData[odString] = new PlayerAfkData
                        {
                            LastPosition = player.Position,
                            LastActiveTime = Time.time,
                            WarningSent = false
                        };
                        continue;
                    }

                    float distance = Vector3.Distance(data.LastPosition, player.Position);

                    if (distance >= _config.MinMoveDistance)
                    {
                        // Player moved — reset
                        data.LastPosition = player.Position;
                        data.LastActiveTime = Time.time;
                        data.WarningSent = false;
                        continue;
                    }

                    float idleTime = Time.time - data.LastActiveTime;

                    // Send warning
                    if (!data.WarningSent && idleTime >= _config.WarningSeconds)
                    {
                        int remaining = _config.AfkThresholdSeconds - (int)idleTime;
                        player.SendBroadcast(
                            $"<color=red><b>[AFK Warning]</b></color>\nYou will be moved to spectator in <color=yellow>{remaining}</color> seconds!\nMove to avoid this.",
                            5);
                        data.WarningSent = true;
                    }

                    // Take action
                    if (idleTime >= _config.AfkThresholdSeconds)
                    {
                        if (_config.MoveToSpectator)
                        {
                            player.SetRole(RoleTypeId.Spectator);
                            player.SendBroadcast(
                                "<color=orange>[AFK]</color> You were moved to spectator for being idle.",
                                8);
                            LabApi.Features.Console.Logger.Info($"[AntiAFK] Moved {player.Nickname} to spectator (AFK for {(int)idleTime}s)");
                        }
                        else if (_config.KickAfk)
                        {
                            LabApi.Features.Console.Logger.Info($"[AntiAFK] Kicked {player.Nickname} (AFK for {(int)idleTime}s)");
                            player.Kick("You were kicked for being AFK.");
                        }

                        data.LastActiveTime = Time.time;
                        data.WarningSent = false;
                    }
                }
            }
        }

        private class PlayerAfkData
        {
            public Vector3 LastPosition;
            public float LastActiveTime;
            public bool WarningSent;
        }
    }
}
