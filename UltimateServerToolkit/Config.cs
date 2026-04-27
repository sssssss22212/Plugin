using System.Collections.Generic;

namespace UltimateServerToolkit
{
    public sealed class Config
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public ProfilesConfig Profiles { get; set; } = new ProfilesConfig();
        public KarmaConfig Karma { get; set; } = new KarmaConfig();
        public AntiRdmConfig AntiRdm { get; set; } = new AntiRdmConfig();
        public WhitelistConfig Whitelist { get; set; } = new WhitelistConfig();
        public NotesConfig Notes { get; set; } = new NotesConfig();
        public RpCommandsConfig RpCommands { get; set; } = new RpCommandsConfig();
        public RoundLogConfig RoundLog { get; set; } = new RoundLogConfig();
        public GmConfig Gm { get; set; } = new GmConfig();
    }

    public sealed class ProfilesConfig
    {
        public bool Enabled { get; set; } = true;

        public bool OverrideDisplayName { get; set; } = true;
        public int MinNameLength { get; set; } = 3;
        public int MaxNameLength { get; set; } = 24;
        public int MaxBioLength { get; set; } = 240;
        public int StartingKarma { get; set; } = 100;
    }

    public sealed class KarmaConfig
    {
        public bool Enabled { get; set; } = true;
        public int RdmPenalty { get; set; } = 25;
        public int TeamKillPenalty { get; set; } = 40;
        public int RoundEndBonus { get; set; } = 5;
        public int LowKarmaThreshold { get; set; } = 25;
        public int CriticalKarmaThreshold { get; set; } = 0;

        public bool KickOnCritical { get; set; } = true;
    }

    public sealed class AntiRdmConfig
    {
        public bool Enabled { get; set; } = true;

        public float TeamDamageWindowSeconds { get; set; } = 2.5f;
        public bool BroadcastWarning { get; set; } = true;
        public string RdmWarningHint { get; set; } = "<color=#ff5555><b>[РП]</b></color> Засчитан RDM. Карма уменьшена.";
    }

    public sealed class WhitelistConfig
    {
        public bool Enabled { get; set; } = false;

        public List<string> RestrictedRoles { get; set; } = new List<string>
        {
            "Scp079",
            "Scp096",
            "Scp106",
            "Scp173",
            "Scp939",
            "Scp3114",
            "Scp049",
        };

        public List<string> AllowedUserIds { get; set; } = new List<string>();
        public string DenyMessage { get; set; } = "<color=#ff5555>Эта SCP-роль только для вайтлиста. Спроси у админа.</color>";
    }

    public sealed class NotesConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxNoteLength { get; set; } = 240;
        public int MaxNotesPerRound { get; set; } = 25;
    }

    public sealed class RpCommandsConfig
    {
        public bool Enabled { get; set; } = true;
        public float MeRangeMeters { get; set; } = 8f;
        public float WhisperRangeMeters { get; set; } = 3f;
        public float LookRangeMeters { get; set; } = 12f;
        public string MeFormat { get; set; } = "<color=#FFD27F><i>* {name} {action}</i></color>";
        public string TryFormat { get; set; } = "<color=#7FBFFF><i>* {name} пытается {action} — <b>{outcome}</b></i></color>";
        public string DoFormat { get; set; } = "<color=#A0FFA0><i>* {action}</i></color>";
        public string LookFormat { get; set; } = "<color=#CCCCCC><i>Видишь {target}: {bio}</i></color>";
        public string WhisperFormat { get; set; } = "<color=#FFA0FF><i>{name} шепчет: {message}</i></color>";
    }

    public sealed class RoundLogConfig
    {
        public bool Enabled { get; set; } = true;
        public bool LogJoinsAndLeaves { get; set; } = true;
        public bool LogDeaths { get; set; } = true;
        public bool LogRoleChanges { get; set; } = false;
    }

    public sealed class GmConfig
    {
        public bool Enabled { get; set; } = true;

        public List<string> GameMasterUserIds { get; set; } = new List<string>();
    }
}
