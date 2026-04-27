using System;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Modules.Profiles
{
    public sealed class ProfileManager
    {
        private readonly UstPlugin _plugin;
        private readonly ProfileStore _store;

        public ProfileManager(UstPlugin plugin, ProfileStore store)
        {
            _plugin = plugin;
            _store = store;
        }

        public RpProfile GetOrCreate(Player p)
        {
            var prof = _store.GetOrCreate(p.UserId, p.Nickname, _plugin.Config.Profiles.StartingKarma);
            prof.LastNickname = p.Nickname ?? prof.LastNickname;
            prof.LastSeenUtc = DateTime.UtcNow;
            _store.MarkDirty();
            return prof;
        }

        public bool TryGet(string userId, out RpProfile profile) =>
            _store.TryGet(userId, out profile);

        public bool TrySetName(Player p, string newName, out string error)
        {
            var cfg = _plugin.Config.Profiles;
            error = null;

            if (string.IsNullOrWhiteSpace(newName))
            {
                error = "Имя пустое.";
                return false;
            }

            newName = newName.Trim();

            if (newName.Length < cfg.MinNameLength || newName.Length > cfg.MaxNameLength)
            {
                error = $"Имя должно быть {cfg.MinNameLength}–{cfg.MaxNameLength} символов.";
                return false;
            }

            var prof = GetOrCreate(p);
            prof.RpName = newName;
            _store.MarkDirty();

            if (cfg.OverrideDisplayName)
                _plugin.DisplayNames.Apply(p);

            return true;
        }

        public bool TrySetBio(Player p, string bio, out string error)
        {
            var cfg = _plugin.Config.Profiles;
            error = null;

            bio = (bio ?? string.Empty).Trim();
            if (bio.Length > cfg.MaxBioLength)
            {
                error = $"Био слишком длинное (макс {cfg.MaxBioLength}).";
                return false;
            }

            var prof = GetOrCreate(p);
            prof.Bio = bio;
            _store.MarkDirty();
            return true;
        }

        public bool TrySetFaction(Player p, string faction, out string error)
        {
            error = null;
            faction = (faction ?? string.Empty).Trim();
            if (faction.Length > 32)
            {
                error = "Имя фракции слишком длинное (макс 32).";
                return false;
            }

            var prof = GetOrCreate(p);
            prof.Faction = faction;
            _store.MarkDirty();
            return true;
        }
    }
}
