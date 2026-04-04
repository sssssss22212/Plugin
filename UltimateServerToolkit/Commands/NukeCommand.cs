using System;
using CommandSystem;
using UltimateServerToolkit.Modules;

namespace UltimateServerToolkit.Commands
{
    public class NukeCommand : ICommand, IUsageProvider
    {
        private readonly AutoNukeModule _module;

        public NukeCommand(AutoNukeModule module)
        {
            _module = module;
        }

        public string Command => "autonuke";
        public string[] Aliases => new[] { "an" };
        public string Description => "Control the auto-nuke system.";
        public string[] Usage => new[] { "action (start/cancel/status)" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: autonuke <start|cancel|status>";
                return false;
            }

            string action = arguments.At(0).ToLower();

            switch (action)
            {
                case "start":
                case "force":
                    _module.ForceStartNuke();
                    response = "Auto-nuke forcefully started!";
                    return true;
                case "cancel":
                case "stop":
                    _module.CancelAutoNuke();
                    response = "Auto-nuke timer cancelled.";
                    return true;
                case "status":
                case "info":
                    float timeLeft = _module.GetTimeUntilNuke();
                    if (timeLeft < 0)
                        response = "No round in progress.";
                    else if (timeLeft <= 0)
                        response = "Auto-nuke already triggered or cancelled.";
                    else
                        response = $"Time until auto-nuke: {timeLeft:F0} seconds ({timeLeft / 60f:F1} minutes)";
                    return true;
                default:
                    response = "Unknown action. Use: start, cancel, or status";
                    return false;
            }
        }
    }
}
