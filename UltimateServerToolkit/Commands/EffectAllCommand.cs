using System;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Commands
{
    public class EffectAllCommand : ICommand, IUsageProvider
    {
        public string Command => "ueffectall";
        public string[] Aliases => new[] { "uea" };
        public string Description => "Apply a status effect to all alive players, or clear all effects.";
        public string[] Usage => new[] { "effect_name|clear", "[intensity 1-255]", "[duration seconds]" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: ueffectall <effect_name|clear> [intensity] [duration]\nExample: ueffectall MovementBoost 50 30";
                return false;
            }

            string effectName = arguments.At(0);

            if (effectName.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                int cleared = 0;
                foreach (var p in Player.ReadyList)
                {
                    if (p != null && p.IsAlive && !p.IsHost)
                    {
                        p.DisableAllEffects();
                        cleared++;
                    }
                }

                string hint = Utils.HintBuilder.Size(
                    Utils.HintBuilder.Bold(Utils.HintBuilder.Color("All effects cleared!", "#00ff00")),
                    20);

                foreach (var p in Player.ReadyList)
                {
                    if (p != null && p.IsReady && !p.IsHost)
                        p.SendHint(hint, 3f);
                }

                response = $"Cleared all effects on {cleared} players.";
                return true;
            }

            byte intensity = 1;
            float duration = 30f;

            if (arguments.Count >= 2 && byte.TryParse(arguments.At(1), out byte parsedIntensity))
                intensity = parsedIntensity;

            if (arguments.Count >= 3 && float.TryParse(arguments.At(2), out float parsedDuration))
                duration = parsedDuration;

            int affected = 0;
            int errors = 0;

            foreach (var p in Player.ReadyList)
            {
                if (p == null || !p.IsAlive || p.IsHost)
                    continue;

                try
                {
                    if (p.TryGetEffect(effectName, out var effect))
                    {
                        p.EnableEffect(effect, intensity, duration, false);
                        affected++;
                    }
                    else
                    {
                        errors++;
                    }
                }
                catch
                {
                    errors++;
                }
            }

            if (affected > 0)
            {
                string hint = Utils.HintBuilder.Size(
                    Utils.HintBuilder.Bold(Utils.HintBuilder.Color($"Effect: {effectName}", "#ff66ff")),
                    20) + "\n" +
                    Utils.HintBuilder.Size(
                        Utils.HintBuilder.Color($"Intensity: {intensity} | Duration: {duration}s", "#aaaaaa"),
                        14);

                foreach (var p in Player.ReadyList)
                {
                    if (p != null && p.IsReady && !p.IsHost)
                        p.SendHint(hint, 3f);
                }
            }

            response = $"Applied {effectName} (intensity={intensity}, duration={duration}s) to {affected} players.";
            if (errors > 0)
                response += $" ({errors} players didn't have the effect available)";

            return affected > 0;
        }
    }
}
