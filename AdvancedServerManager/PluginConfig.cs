using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AdvancedServerManager
{
    /// <summary>
    /// Main configuration for the Advanced Server Manager plugin.
    /// </summary>
    public class PluginConfig
    {
        /// <summary>
        /// Anti-AFK module settings.
        /// </summary>
        public AntiAfkConfig AntiAfk { get; set; } = new AntiAfkConfig();

        /// <summary>
        /// Auto team balance module settings.
        /// </summary>
        public AutoBalanceConfig AutoBalance { get; set; } = new AutoBalanceConfig();

        /// <summary>
        /// Player statistics module settings.
        /// </summary>
        public StatsConfig Stats { get; set; } = new StatsConfig();

        /// <summary>
        /// Custom spawn loadout settings.
        /// </summary>
        public LoadoutConfig Loadouts { get; set; } = new LoadoutConfig();

        /// <summary>
        /// Welcome message settings.
        /// </summary>
        public WelcomeConfig Welcome { get; set; } = new WelcomeConfig();
    }

    public class AntiAfkConfig
    {
        /// <summary>
        /// Whether the Anti-AFK module is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Time in seconds before a player is considered AFK.
        /// </summary>
        public int AfkThresholdSeconds { get; set; } = 120;

        /// <summary>
        /// Time in seconds before an AFK warning is sent.
        /// </summary>
        public int WarningSeconds { get; set; } = 90;

        /// <summary>
        /// Whether to move AFK players to spectator instead of kicking.
        /// </summary>
        public bool MoveToSpectator { get; set; } = true;

        /// <summary>
        /// Whether to kick AFK players if MoveToSpectator is false.
        /// </summary>
        public bool KickAfk { get; set; } = false;

        /// <summary>
        /// Minimum distance in units a player must move to not be considered AFK.
        /// </summary>
        public float MinMoveDistance { get; set; } = 2.0f;

        /// <summary>
        /// Whether spectators are immune to AFK detection.
        /// </summary>
        public bool IgnoreSpectators { get; set; } = true;
    }

    public class AutoBalanceConfig
    {
        /// <summary>
        /// Whether the Auto Balance module is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum allowed difference between human team sizes before balancing.
        /// </summary>
        public int MaxTeamDifference { get; set; } = 3;

        /// <summary>
        /// Whether to broadcast a message when teams are balanced.
        /// </summary>
        public bool BroadcastOnBalance { get; set; } = true;
    }

    public class StatsConfig
    {
        /// <summary>
        /// Whether the Stats module is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether to show stats to the player on round end.
        /// </summary>
        public bool ShowOnRoundEnd { get; set; } = true;

        /// <summary>
        /// Whether to track kill/death stats.
        /// </summary>
        public bool TrackKills { get; set; } = true;

        /// <summary>
        /// Whether to track item usage stats.
        /// </summary>
        public bool TrackItems { get; set; } = true;

        /// <summary>
        /// Whether to track escape stats.
        /// </summary>
        public bool TrackEscapes { get; set; } = true;
    }

    public class LoadoutConfig
    {
        /// <summary>
        /// Whether the Loadout module is enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Custom loadouts per role. Key is RoleTypeId name, value is list of ItemType names.
        /// </summary>
        public Dictionary<string, List<string>> RoleLoadouts { get; set; } = new Dictionary<string, List<string>>();
    }

    public class WelcomeConfig
    {
        /// <summary>
        /// Whether to show a welcome broadcast when a player joins.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// The welcome message. Use %player% for the player's nickname.
        /// </summary>
        public string Message { get; set; } = "<b>Welcome, <color=yellow>%player%</color>!</b>\nAdvanced Server Manager is active.";

        /// <summary>
        /// Duration of the welcome broadcast in seconds.
        /// </summary>
        public ushort Duration { get; set; } = 6;
    }
}
