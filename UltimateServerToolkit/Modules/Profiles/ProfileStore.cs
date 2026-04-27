using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using LabApi.Features.Console;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UltimateServerToolkit.Modules.Profiles
{
    /// <summary>
    /// Loads, caches, and persists <see cref="RpProfile"/>s to a single YAML file.
    /// All operations are in-memory and flushed on demand or on disable.
    /// </summary>
    public sealed class ProfileStore
    {
        private readonly string _path;
        private readonly ConcurrentDictionary<string, RpProfile> _byUserId =
            new ConcurrentDictionary<string, RpProfile>(StringComparer.OrdinalIgnoreCase);

        private readonly ISerializer _serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        private readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        private bool _dirty;

        public ProfileStore(string dataDir)
        {
            Directory.CreateDirectory(dataDir);
            _path = Path.Combine(dataDir, "profiles.yml");
            Load();
        }

        public RpProfile GetOrCreate(string userId, string nickname, int startingKarma)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId required.", nameof(userId));

            return _byUserId.GetOrAdd(userId, id =>
            {
                _dirty = true;
                return new RpProfile
                {
                    UserId = id,
                    LastNickname = nickname ?? string.Empty,
                    Karma = startingKarma,
                };
            });
        }

        public bool TryGet(string userId, out RpProfile profile) =>
            _byUserId.TryGetValue(userId, out profile);

        public IReadOnlyCollection<RpProfile> All => (IReadOnlyCollection<RpProfile>)_byUserId.Values;

        public void MarkDirty() => _dirty = true;

        public void SaveAll(bool force = false)
        {
            if (!force && !_dirty) return;
            try
            {
                var dump = new Dictionary<string, RpProfile>(_byUserId, StringComparer.OrdinalIgnoreCase);
                File.WriteAllText(_path, _serializer.Serialize(dump));
                _dirty = false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[UST/ProfileStore] Failed to save profiles: {ex.Message}");
            }
        }

        private void Load()
        {
            if (!File.Exists(_path)) return;

            try
            {
                var raw = File.ReadAllText(_path);
                if (string.IsNullOrWhiteSpace(raw)) return;

                var data = _deserializer.Deserialize<Dictionary<string, RpProfile>>(raw);
                if (data == null) return;

                foreach (var kvp in data)
                {
                    if (kvp.Value == null) continue;
                    if (string.IsNullOrEmpty(kvp.Value.UserId))
                        kvp.Value.UserId = kvp.Key;
                    _byUserId[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[UST/ProfileStore] Failed to load profiles ({ex.Message}). Starting fresh.");
            }
        }
    }
}
