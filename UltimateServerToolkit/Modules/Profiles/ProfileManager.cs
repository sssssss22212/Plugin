using System;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Modules.Profiles
{
    /// <summary>High-level API used by commands and event handlers to mutate profiles.</summary>
    public sealed class ProfileManager
    {
        private readonly UstPlugin _plugin;
        private readonly ProfileStore _store;

        public ProfileManager(UstPlugin plugin, ProfileStore store)
        {
            _plugin = plugin;
            _store = store;
        }

        public RpProfile GetOrCreate(Player player)
        {
            var profile = _store.GetOrCreate(
                player.UserId,
                player.Nickname,
                _plugin.Config.Profiles.StartingKarma);

            profile.LastNickname = player.Nickname ?? profile.LastNickname;
            profile.LastSeenUtc = DateTime.UtcNow;
            _store.MarkDirty();
            return profile;
        }

        public bool TryGet(string userId, out RpProfile profile) =>
            _store.TryGet(userId, out profile);

        public bool TrySetName(Player player, string newName, out string error)
        {
            var cfg = _plugin.Config.Profiles;
            error = null;

            if (string.IsNullOrWhiteSpace(newName))
            {
                error = "Name is empty.";
                return false;
            }

            newName = newName.Trim();

            if (newName.Length < cfg.MinNameLength || newName.Length > cfg.MaxNameLength)
            {
                error = $"Name must be {cfg.MinNameLength}-{cfg.MaxNameLength} characters.";
                return false;
            }

            var profile = GetOrCreate(player);
            profile.RpName = newName;
            _store.MarkDirty();

            if (cfg.OverrideDisplayName)
                _plugin.DisplayNames.Apply(player);

            return true;
        }

        public bool TrySetBio(Player player, string bio, out string error)
        {
            var cfg = _plugin.Config.Profiles;
            error = null;

            bio = (bio ?? string.Empty).Trim();
            if (bio.Length > cfg.MaxBioLength)
            {
                error = $"Bio too long (max {cfg.MaxBioLength} chars).";
                return false;
            }

            var profile = GetOrCreate(player);
            profile.Bio = bio;
            _store.MarkDirty();
            return true;
        }

        public bool TrySetFaction(Player player, string faction, out string error)
        {
            error = null;
            faction = (faction ?? string.Empty).Trim();
            if (faction.Length > 32)
            {
                error = "Faction name too long (max 32 chars).";
                return false;
            }

            var profile = GetOrCreate(player);
            profile.Faction = faction;
            _store.MarkDirty();
            return true;
        }
    }
}
