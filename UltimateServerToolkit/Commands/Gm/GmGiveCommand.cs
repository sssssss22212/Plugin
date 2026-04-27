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
        public string Description => "Выдать предмет. Использование: gm give <игрок> <ItemType>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count < 2) { response = "Использование: gm give <игрок> <ItemType>"; return false; }

            var target = CommandUtil.FindByQuery(arguments.Array[arguments.Offset]);
            if (target == null) { response = "Игрок не найден."; return false; }

            var itemName = arguments.Array[arguments.Offset + 1];
            if (!Enum.TryParse(itemName, ignoreCase: true, result: out ItemType item))
            {
                response = $"Неизвестный ItemType '{itemName}'.";
                return false;
            }

            target.AddItem(item, InventorySystem.Items.ItemAddReason.AdminCommand);
            response = $"Выдал {item} -> {target.DisplayName}.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmHealCommand : ICommand
    {
        public string Command => "heal";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Полностью вылечить. Использование: gm heal <игрок>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Использование: gm heal <игрок>"; return false; }

            var target = CommandUtil.FindByQuery(string.Join(" ", arguments.Array, arguments.Offset, arguments.Count));
            if (target == null) { response = "Игрок не найден."; return false; }

            target.Heal(target.MaxHealth);
            response = $"Вылечил {target.DisplayName}.";
            return true;
        }
    }

    [CommandHandler(typeof(GmParentCommand))]
    public sealed class GmGodCommand : ICommand
    {
        public string Command => "god";
        public string[] Aliases => new[] { "godmode" };
        public string Description => "Переключить бессмертие. Использование: gm god <игрок>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!GmParentCommand.RequireGm(sender, out response)) return false;
            if (arguments.Count == 0) { response = "Использование: gm god <игрок>"; return false; }

            var target = CommandUtil.FindByQuery(string.Join(" ", arguments.Array, arguments.Offset, arguments.Count));
            if (target == null) { response = "Игрок не найден."; return false; }

            target.IsGodModeEnabled = !target.IsGodModeEnabled;
            response = $"{target.DisplayName} godmode = {target.IsGodModeEnabled}";
            return true;
        }
    }
}
