using System;
using System.Collections.Generic;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using UltimateServerToolkit.Utils;

namespace UltimateServerToolkit.Modules
{
    public class PlayerReportsModule
    {
        private readonly PlayerReportsConfig _config;
        private readonly Dictionary<string, List<PlayerReport>> _reports = new Dictionary<string, List<PlayerReport>>();
        private readonly Dictionary<string, int> _reportCounts = new Dictionary<string, int>();

        public PlayerReportsModule(PlayerReportsConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            ServerEvents.RoundStarted += OnRoundStarted;
            Logger.Info("[Reports] Module enabled.");
        }

        public void Disable()
        {
            ServerEvents.RoundStarted -= OnRoundStarted;
            Logger.Info("[Reports] Module disabled.");
        }

        private void OnRoundStarted()
        {
            _reportCounts.Clear();
        }

        public bool SubmitReport(Player reporter, Player target, string reason)
        {
            if (reporter == null || target == null)
                return false;

            string reporterId = reporter.UserId;

            if (!_reportCounts.TryGetValue(reporterId, out int count))
                count = 0;

            if (count >= _config.MaxReportsPerRound)
            {
                reporter.SendHint(
                    HintBuilder.Size(
                        HintBuilder.Color($"You have reached the report limit ({_config.MaxReportsPerRound} per round)", "#ff3333"),
                        16),
                    3f);
                return false;
            }

            _reportCounts[reporterId] = count + 1;

            string targetId = target.UserId;
            if (!_reports.TryGetValue(targetId, out List<PlayerReport> targetReports))
            {
                targetReports = new List<PlayerReport>();
                _reports[targetId] = targetReports;
            }

            targetReports.Add(new PlayerReport
            {
                ReporterName = reporter.Nickname,
                ReporterId = reporterId,
                TargetName = target.Nickname,
                TargetId = targetId,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            });

            // Confirm to reporter
            reporter.SendHint(
                HintBuilder.Size(
                    HintBuilder.Bold(HintBuilder.Color("Report submitted!", "#00ff00")),
                    20) + "\n" +
                HintBuilder.Size(
                    HintBuilder.Color($"Target: {target.Nickname}", "#aaaaaa") + "\n" +
                    HintBuilder.Color($"Reason: {reason}", "#aaaaaa"),
                    14),
                4f);

            // Notify admins via admin chat
            Server.SendAdminChatMessage(
                $"[Report] {reporter.Nickname} reported {target.Nickname}: {reason} (Reports: {targetReports.Count})",
                false);

            // If target has many reports, warn admins
            if (targetReports.Count >= 3)
            {
                Server.SendAdminChatMessage(
                    $"[Report] WARNING: {target.Nickname} has {targetReports.Count} reports this session!",
                    false);
            }

            Logger.Info($"[Reports] {reporter.Nickname} reported {target.Nickname}: {reason}");
            return true;
        }

        public List<PlayerReport> GetReportsFor(string userId)
        {
            return _reports.TryGetValue(userId, out List<PlayerReport> reports) ? reports : new List<PlayerReport>();
        }

        public int GetTotalReports()
        {
            int total = 0;
            foreach (var kvp in _reports)
                total += kvp.Value.Count;
            return total;
        }
    }

    public class PlayerReport
    {
        public string ReporterName { get; set; }
        public string ReporterId { get; set; }
        public string TargetName { get; set; }
        public string TargetId { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
