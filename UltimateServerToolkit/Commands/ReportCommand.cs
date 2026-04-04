using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using UltimateServerToolkit.Modules;

namespace UltimateServerToolkit.Commands
{
    public class ReportCommand : ICommand, IUsageProvider
    {
        private readonly PlayerReportsModule _module;

        public ReportCommand(PlayerReportsModule module)
        {
            _module = module;
        }

        public string Command => "report";
        public string[] Aliases => new[] { "rep" };
        public string Description => "Report a player for rule violations.";
        public string[] Usage => new[] { "player_id", "reason" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: .report <player_id> <reason>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int targetId))
            {
                response = "Invalid player ID.";
                return false;
            }

            // Build reason from remaining args
            string reason = "";
            for (int i = 1; i < arguments.Count; i++)
            {
                if (reason.Length > 0) reason += " ";
                reason += arguments.At(i);
            }

            // Find reporter
            Player reporter = null;
            var cmdSender = sender as CommandSender;
            string senderName = cmdSender?.LogName ?? "";

            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.Nickname == senderName)
                {
                    reporter = p;
                    break;
                }
            }

            if (reporter == null)
            {
                response = "Could not identify you.";
                return false;
            }

            // Find target
            Player target = null;
            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.PlayerId == targetId)
                {
                    target = p;
                    break;
                }
            }

            if (target == null)
            {
                response = $"Player with ID {targetId} not found.";
                return false;
            }

            if (target.UserId == reporter.UserId)
            {
                response = "You cannot report yourself.";
                return false;
            }

            bool success = _module.SubmitReport(reporter, target, reason);

            if (success)
                response = $"Report submitted against {target.Nickname}.";
            else
                response = "Failed to submit report. You may have reached the limit.";

            return success;
        }
    }
}
