using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace UltimateServerToolkit.Commands.Gm
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class GmParentCommand : ParentCommand
    {
        public override string Command => "gm";
        public override string[] Aliases => new[] { "rpgm" };
        public override string Description => "GM-инструменты UltimateServerToolkit.";

        public GmParentCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            var sb = new StringBuilder();
            sb.AppendLine("GM-команды (gm <sub>):");
            foreach (var sub in AllCommands)
                sb.AppendLine($"  {sub.Command,-12} {sub.Description}");
            response = sb.ToString();
            return true;
        }

        internal static bool RequireGm(ICommandSender sender, out string error)
        {
            error = null;

            var gm = UstPlugin.Instance?.Config?.Gm;
            if (gm != null && !gm.Enabled) { error = "GM-инструменты выключены."; return false; }

            var p = Player.Get(sender);
            if (p == null) return true;
            if (p.RemoteAdminAccess) return true;

            var allow = gm?.GameMasterUserIds;
            if (allow != null && allow.Contains(p.UserId, StringComparer.OrdinalIgnoreCase)) return true;

            error = "Нужен Remote Admin или вайтлист GameMasterUserIds.";
            return false;
        }
    }
}
