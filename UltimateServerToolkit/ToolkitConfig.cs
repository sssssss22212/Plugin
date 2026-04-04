using System.Collections.Generic;

namespace UltimateServerToolkit
{
    public class ToolkitConfig
    {
        public HudConfig Hud { get; set; } = new HudConfig();
        public KillstreakConfig Killstreaks { get; set; } = new KillstreakConfig();
        public AutoNukeConfig AutoNuke { get; set; } = new AutoNukeConfig();
        public ScpInfoConfig ScpInfo { get; set; } = new ScpInfoConfig();
        public RespawnTimerConfig RespawnTimer { get; set; } = new RespawnTimerConfig();
        public DoorLockdownConfig DoorLockdown { get; set; } = new DoorLockdownConfig();
        public CassieAutoConfig CassieAuto { get; set; } = new CassieAutoConfig();
        public AdminToolsConfig AdminTools { get; set; } = new AdminToolsConfig();
        public LobbyConfig Lobby { get; set; } = new LobbyConfig();
        public PlayerReportsConfig Reports { get; set; } = new PlayerReportsConfig();
    }

    public class HudConfig
    {
        public bool Enabled { get; set; } = true;
        public float UpdateInterval { get; set; } = 1.0f;
        public bool ShowServerName { get; set; } = true;
        public bool ShowTps { get; set; } = true;
        public bool ShowPlayerCount { get; set; } = true;
        public bool ShowRoundTimer { get; set; } = true;
        public bool ShowRole { get; set; } = true;
        public bool ShowHealth { get; set; } = true;
        public string ServerDisplayName { get; set; } = "SCP:SL Server";
        public string HudColor { get; set; } = "#00ffcc";
        public string AccentColor { get; set; } = "#ff6600";
    }

    public class KillstreakConfig
    {
        public bool Enabled { get; set; } = true;
        public int DoubleKillThreshold { get; set; } = 2;
        public int TripleKillThreshold { get; set; } = 3;
        public int UltraKillThreshold { get; set; } = 5;
        public int GodlikeThreshold { get; set; } = 8;
        public float KillstreakTimeout { get; set; } = 10f;
        public bool AnnounceToAll { get; set; } = true;
        public bool GrantHealthBonus { get; set; } = true;
        public float HealthBonusPerKill { get; set; } = 10f;
    }

    public class AutoNukeConfig
    {
        public bool Enabled { get; set; } = true;
        public float AutoNukeTimeMinutes { get; set; } = 25f;
        public float WarningTimeMinutes { get; set; } = 20f;
        public bool LockAfterStart { get; set; } = true;
        public bool AnnounceWarning { get; set; } = true;
    }

    public class ScpInfoConfig
    {
        public bool Enabled { get; set; } = true;
        public float HintDuration { get; set; } = 8f;
        public bool ShowOnSpawn { get; set; } = true;
        public bool ShowTargetCount { get; set; } = true;
    }

    public class RespawnTimerConfig
    {
        public bool Enabled { get; set; } = true;
        public string TimerColor { get; set; } = "#ffcc00";
    }

    public class DoorLockdownConfig
    {
        public bool Enabled { get; set; } = true;
        public float LockdownDurationSeconds { get; set; } = 30f;
    }

    public class CassieAutoConfig
    {
        public bool Enabled { get; set; } = true;
        public bool AnnounceScpKills { get; set; } = true;
        public bool AnnounceGenerators { get; set; } = true;
        public bool AnnounceDecontamination { get; set; } = true;
        public bool AnnounceEscapes { get; set; } = true;
    }

    public class AdminToolsConfig
    {
        public bool Enabled { get; set; } = true;
        public bool LogCommands { get; set; } = true;
    }

    public class LobbyConfig
    {
        public bool Enabled { get; set; } = true;
        public string WelcomeMessage { get; set; } = "<color=#00ffcc><b>Welcome to the server!</b></color>\n<color=#aaaaaa>Round starts soon...</color>";
        public float MessageDuration { get; set; } = 5f;
    }

    public class PlayerReportsConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxReportsPerRound { get; set; } = 3;
    }
}
