using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommandSystem;
using LabApi.Features.Wrappers;


namespace UltimateServerToolkit.Commands.Gm
{
    /// <summary>
    /// Parent command for Game-Master tools. Lives in RA panel.
    /// Subcommands attach via <c>[CommandHandler(typeof(GmParentCommand))]</c>.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class GmParentCommand : ParentCommand
    {
        public override string Command => "gm";
        public override string[] Aliases => new[] { "rpgm" };
        public override string Description => "RP Game-Master tools (UltimateServerToolkit).";

        public GmParentCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands() { /* sub commands auto-registered by CommandHandler attr */ }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
        {
            var sb = new StringBuilder();
            sb.AppendLine("UST Game-Master commands (use 'gm <sub>'):");
            foreach (var sub in AllCommands)
                sb.AppendLine($"  {sub.Command,-12} {sub.Description}");
            response = sb.ToString();
            return true;
        }

        internal static bool RequireGm(ICommandSender sender, out string error)
        {
            error = null;

            // Anything that isn't an in-game player is server console / RA — always allowed.
            var player = Player.Get(sender);
            if (player == null) return true;

            if (player.RemoteAdminAccess) return true;

            var allowed = UstPlugin.Instance?.Config?.Gm?.GameMasterUserIds;
            if (allowed != null && allowed.Contains(player.UserId))
                return true;

            error = "GM tools require Remote Admin or a whitelisted Game-Master UserId.";
            return false;
        }
    }
}
