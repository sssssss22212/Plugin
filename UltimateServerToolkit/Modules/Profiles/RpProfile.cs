using System;

namespace UltimateServerToolkit.Modules.Profiles
{
    /// <summary>
    /// Persistent per-user RP data. Keyed by <see cref="UserId"/>.
    /// Stored as YAML in the data directory.
    /// </summary>
    public sealed class RpProfile
    {
        public string UserId { get; set; } = string.Empty;
        public string LastNickname { get; set; } = string.Empty;
        public string RpName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Faction { get; set; } = string.Empty;
        public int Karma { get; set; }
        public int RdmCount { get; set; }
        public int TeamKillCount { get; set; }
        public DateTime FirstSeenUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;

        public bool HasRpName => !string.IsNullOrWhiteSpace(RpName);
        public bool HasBio => !string.IsNullOrWhiteSpace(Bio);
    }
}
