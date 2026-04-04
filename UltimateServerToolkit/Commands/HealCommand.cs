using System;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Commands
{
    public class HealCommand : ICommand, IUsageProvider
    {
        public string Command => "uheal";
        public string[] Aliases => new[] { "uh" };
        public string Description => "Heal a player to full HP or a specific amount.";
        public string[] Usage => new[] { "player_id", "[amount]" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: uheal <player_id> [amount]";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int playerId))
            {
                response = "Invalid player ID.";
                return false;
            }

            Player target = null;
            foreach (var p in Player.ReadyList)
            {
                if (p != null && p.PlayerId == playerId)
                {
                    target = p;
                    break;
                }
            }

            if (target == null)
            {
                response = $"Player with ID {playerId} not found.";
                return false;
            }

            if (!target.IsAlive)
            {
                response = $"{target.Nickname} is dead.";
                return false;
            }

            if (arguments.Count >= 2 && float.TryParse(arguments.At(1), out float amount))
            {
                target.Health = Math.Min(target.Health + amount, target.MaxHealth);
                response = $"Healed {target.Nickname} by {amount} HP. Current: {target.Health:F0}/{target.MaxHealth:F0}";
            }
            else
            {
                target.Health = target.MaxHealth;
                response = $"Fully healed {target.Nickname} ({target.MaxHealth:F0} HP).";
            }

            target.SendHint(
                Utils.HintBuilder.Size(
                    Utils.HintBuilder.Bold(Utils.HintBuilder.Color("HEALED", "#00ff00")),
                    24),
                2f);

            return true;
        }
    }
}
