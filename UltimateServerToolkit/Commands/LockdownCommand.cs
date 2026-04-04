using System;
using CommandSystem;
using MapGeneration;
using UltimateServerToolkit.Modules;

namespace UltimateServerToolkit.Commands
{
    public class LockdownCommand : ICommand, IUsageProvider
    {
        private readonly DoorLockdownModule _module;

        public LockdownCommand(DoorLockdownModule module)
        {
            _module = module;
        }

        public string Command => "lockdown";
        public string[] Aliases => new[] { "ld" };
        public string Description => "Lock all doors in a zone or the entire facility.";
        public string[] Usage => new[] { "zone (lcz/hcz/ez/surface/all)" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: lockdown <lcz|hcz|ez|surface|all>";
                return false;
            }

            if (_module.IsLockdownActive)
            {
                response = "A lockdown is already in progress!";
                return false;
            }

            string zone = arguments.At(0).ToLower();

            switch (zone)
            {
                case "lcz":
                case "light":
                    _module.StartLockdown(FacilityZone.LightContainment);
                    response = "Light Containment Zone lockdown initiated!";
                    return true;
                case "hcz":
                case "heavy":
                    _module.StartLockdown(FacilityZone.HeavyContainment);
                    response = "Heavy Containment Zone lockdown initiated!";
                    return true;
                case "ez":
                case "entrance":
                    _module.StartLockdown(FacilityZone.Entrance);
                    response = "Entrance Zone lockdown initiated!";
                    return true;
                case "surface":
                    _module.StartLockdown(FacilityZone.Surface);
                    response = "Surface lockdown initiated!";
                    return true;
                case "all":
                case "full":
                    _module.StartFullLockdown();
                    response = "FULL FACILITY lockdown initiated!";
                    return true;
                default:
                    response = "Unknown zone. Use: lcz, hcz, ez, surface, or all";
                    return false;
            }
        }
    }
}
