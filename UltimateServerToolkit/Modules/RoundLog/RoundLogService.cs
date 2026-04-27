using System;
using System.Collections.Generic;
using System.IO;
using LabApi.Features.Console;

namespace UltimateServerToolkit.Modules.RoundLog
{
    /// <summary>
    /// Append-only round log writer. One file per round, named by start timestamp.
    /// Buffers entries in memory and flushes on round end / disable.
    /// </summary>
    public sealed class RoundLogService
    {
        private readonly UstPlugin _plugin;
        private readonly string _logDir;
        private readonly List<string> _buffer = new List<string>(1024);
        private string _currentFile;
        private DateTime _roundStartUtc = DateTime.UtcNow;

        public RoundLogService(UstPlugin plugin, string dataDir)
        {
            _plugin = plugin;
            _logDir = Path.Combine(dataDir, "rounds");
            Directory.CreateDirectory(_logDir);
        }

        public bool Enabled => _plugin.Config.RoundLog.Enabled;

        public void StartRound()
        {
            Flush();
            _roundStartUtc = DateTime.UtcNow;
            _currentFile = Path.Combine(_logDir, _roundStartUtc.ToString("yyyy-MM-dd_HH-mm-ss") + ".log");
            Append("ROUND_STARTED");
        }

        public void EndRound(string reason = null)
        {
            Append(reason == null ? "ROUND_ENDED" : "ROUND_ENDED: " + reason);
            Flush();
        }

        public void Append(string entry)
        {
            if (!Enabled) return;
            var line = $"{DateTime.UtcNow:HH:mm:ss.fff} | {entry}";
            lock (_buffer)
            {
                _buffer.Add(line);
                if (_buffer.Count >= 256) FlushNoLock();
            }
        }

        public void Flush()
        {
            lock (_buffer) FlushNoLock();
        }

        private void FlushNoLock()
        {
            if (_buffer.Count == 0 || string.IsNullOrEmpty(_currentFile)) return;
            try
            {
                File.AppendAllLines(_currentFile, _buffer);
                _buffer.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error($"[UST/RoundLog] Flush failed: {ex.Message}");
            }
        }
    }
}
