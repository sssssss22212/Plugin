using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace UltimateServerToolkit.Commands.Gm
{
    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmGiveCommand : ICommand
    {
        public string Command => "give";
        public string[] Aliases => new[] { "g" };
        public string Description => "Give an item to a player. Usage: gm give <player> <ItemType>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count < 2) { response = "Usage: gm give <player> <ItemType>"; return false; }

            var target = CommandUtil.FindByQuery(arguments.Array[arguments.Offset]);
            if (target == null) { response = "Target not found."; return false; }

            var itemName = arguments.Array[arguments.Offset + 1];
            if (!Enum.TryParse(itemName, ignoreCase: true, result: out ItemType item))
            {
                response = $"Unknown ItemType '{itemName}'.";
                return false;
            }

            target.AddItem(item, InventorySystem.Items.ItemAddReason.AdminCommand);
            response = $"Gave {item} to {target.DisplayName}.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmHealCommand : ICommand
    {
        public string Command => "heal";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Heal a player to max HP. Usage: gm heal <player>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Usage: gm heal <player>"; return false; }

            var target = CommandUtil.FindByQuery(string.Join(" ", arguments.Array, arguments.Offset, arguments.Count));
            if (target == null) { response = "Target not found."; return false; }

            target.Heal(target.MaxHealth);
            response = $"Healed {target.DisplayName}.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmGodCommand : ICommand
    {
        public string Command => "god";
        public string[] Aliases => new[] { "godmode" };
        public string Description => "Toggle godmode on a player. Usage: gm god <player>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Usage: gm god <player>"; return false; }

            var target = CommandUtil.FindByQuery(string.Join(" ", arguments.Array, arguments.Offset, arguments.Count));
            if (target == null) { response = "Target not found."; return false; }

            target.IsGodModeEnabled = !target.IsGodModeEnabled;
            response = $"{target.DisplayName} godmode = {target.IsGodModeEnabled}";
            return true;
        }
    }
}
