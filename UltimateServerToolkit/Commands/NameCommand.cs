using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace UltimateServerToolkit.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class NameCommand : ICommand
    {
        public string Command => "rpname";
        public string[] Aliases => new[] { "rname", "name" };
        public string Description => "Установить РП-имя персонажа. Использование: .rpname <имя>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var newName = CommandUtil.Join(arguments).Trim();
            if (!plugin.Profiles.TrySetName(p, newName, out var err))
            {
                response = err ?? "Не удалось установить имя.";
                return false;
            }

            response = $"РП-имя: {newName}";
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class BioCommand : ICommand
    {
        public string Command => "bio";
        public string[] Aliases => new[] { "rpbio" };
        public string Description => "Био, которое видят при .look. Использование: .bio <текст>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var text = CommandUtil.Join(arguments).Trim();
            if (!plugin.Profiles.TrySetBio(p, text, out var err))
            {
                response = err ?? "Не удалось установить био.";
                return false;
            }

            response = string.IsNullOrEmpty(text) ? "Био очищено." : "Био обновлено.";
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class FactionCommand : ICommand
    {
        public string Command => "rpfaction";
        public string[] Aliases => new[] { "faction" };
        public string Description => "Установить РП-фракцию. Использование: .rpfaction <текст>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var text = CommandUtil.Join(arguments).Trim();
            if (!plugin.Profiles.TrySetFaction(p, text, out var err))
            {
                response = err ?? "Не удалось установить фракцию.";
                return false;
            }

            response = string.IsNullOrEmpty(text) ? "Фракция очищена." : $"Фракция: {text}";
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class KarmaCommand : ICommand
    {
        public string Command => "karma";
        public string[] Aliases => new[] { "rpkarma" };
        public string Description => "Показать карму и РП-статистику.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            if (!CommandUtil.RequirePlugin(out var plugin, out response)) return false;
            if (!CommandUtil.RequirePlayer(sender, out var p, out response)) return false;

            var prof = plugin.Profiles.GetOrCreate(p);
            response =
                $"Карма: {prof.Karma}\n" +
                $"RDM: {prof.RdmCount}\n" +
                $"Тимкиллы: {prof.TeamKillCount}\n" +
                $"РП-имя: {(prof.HasRpName ? prof.RpName : "(не задано)")}\n" +
                $"С нами с: {prof.FirstSeenUtc:yyyy-MM-dd}";
            return true;
        }
    }
}
