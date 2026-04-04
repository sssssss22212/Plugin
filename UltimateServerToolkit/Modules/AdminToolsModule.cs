using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;

namespace UltimateServerToolkit.Modules
{
    public class AdminToolsModule
    {
        private readonly AdminToolsConfig _config;

        public AdminToolsModule(AdminToolsConfig config)
        {
            _config = config;
        }

        public void Enable()
        {
            if (_config.LogCommands)
            {
                ServerEvents.CommandExecuted += OnCommandExecuted;
            }
            ServerEvents.BanIssued += OnBanIssued;
            Logger.Info("[AdminTools] Module enabled.");
        }

        public void Disable()
        {
            ServerEvents.CommandExecuted -= OnCommandExecuted;
            ServerEvents.BanIssued -= OnBanIssued;
            Logger.Info("[AdminTools] Module disabled.");
        }

        private void OnCommandExecuted(CommandExecutedEventArgs ev)
        {
            if (ev.Sender == null)
                return;

            string senderName = ev.Sender.LogName ?? "Unknown";
            string commandName = ev.CommandName ?? "unknown";
            string success = ev.ExecutedSuccessfully ? "SUCCESS" : "FAILED";

            Logger.Info($"[AdminTools] CMD [{success}] {senderName} -> {commandName}");
        }

        private void OnBanIssued(BanIssuedEventArgs ev)
        {
            Logger.Warn($"[AdminTools] BAN ISSUED: {ev.BanDetails.Id} | Reason: {ev.BanDetails.Reason}");
        }
    }
}
