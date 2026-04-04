using System.Text;

namespace UltimateServerToolkit.Utils
{
    /// <summary>
    /// Utility class for building beautiful rich-text hints with colors, sizes, and formatting.
    /// SCP:SL supports Unity rich text tags in hints.
    /// </summary>
    public static class HintBuilder
    {
        public static string Color(string text, string hexColor)
        {
            return $"<color={hexColor}>{text}</color>";
        }

        public static string Bold(string text)
        {
            return $"<b>{text}</b>";
        }

        public static string Italic(string text)
        {
            return $"<i>{text}</i>";
        }

        public static string Size(string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }

        public static string Header(string text, string color = "#00ffcc", int size = 28)
        {
            return Size(Bold(Color(text, color)), size);
        }

        public static string SubHeader(string text, string color = "#ffcc00", int size = 22)
        {
            return Size(Color(text, color), size);
        }

        public static string Body(string text, string color = "#ffffff", int size = 18)
        {
            return Size(Color(text, color), size);
        }

        public static string Small(string text, string color = "#aaaaaa", int size = 14)
        {
            return Size(Color(text, color), size);
        }

        public static string Separator(string color = "#555555", int width = 30)
        {
            return Size(Color(new string('─', width), color), 12);
        }

        public static string ProgressBar(float current, float max, int length = 20, string fillColor = "#00ff00", string emptyColor = "#333333")
        {
            float ratio = max > 0 ? current / max : 0f;
            int filled = (int)(ratio * length);
            if (filled < 0) filled = 0;
            if (filled > length) filled = length;
            int empty = length - filled;

            string filledStr = Color(new string('█', filled), fillColor);
            string emptyStr = Color(new string('░', empty), emptyColor);
            return filledStr + emptyStr;
        }

        public static string HealthBar(float hp, float maxHp)
        {
            string color;
            float ratio = maxHp > 0 ? hp / maxHp : 0f;
            if (ratio > 0.6f) color = "#00ff00";
            else if (ratio > 0.3f) color = "#ffcc00";
            else color = "#ff3333";

            return ProgressBar(hp, maxHp, 15, color, "#333333");
        }

        public static string KillstreakBanner(string title, string subtitle, string color)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Size(Bold(Color($"★ {title} ★", color)), 32));
            sb.AppendLine(Size(Color(subtitle, "#ffffff"), 18));
            return sb.ToString();
        }

        public static string WarningBanner(string title, string message)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Size(Bold(Color($"⚠ {title} ⚠", "#ff3333")), 30));
            sb.AppendLine(Size(Color(message, "#ffcc00"), 20));
            return sb.ToString();
        }

        public static string InfoPanel(string title, string[] lines, string titleColor = "#00ffcc", string bodyColor = "#ffffff")
        {
            var sb = new StringBuilder();
            sb.AppendLine(Separator());
            sb.AppendLine(Header(title, titleColor, 24));
            sb.AppendLine(Separator());
            foreach (string line in lines)
            {
                sb.AppendLine(Body(line, bodyColor, 16));
            }
            sb.AppendLine(Separator());
            return sb.ToString();
        }

        public static string StatLine(string label, string value, string labelColor = "#aaaaaa", string valueColor = "#ffffff")
        {
            return Color(label + ": ", labelColor) + Bold(Color(value, valueColor));
        }

        public static string RoleName(PlayerRoles.RoleTypeId role)
        {
            switch (role)
            {
                case PlayerRoles.RoleTypeId.ClassD: return Color("Class-D", "#ff8c00");
                case PlayerRoles.RoleTypeId.Scientist: return Color("Scientist", "#ffff00");
                case PlayerRoles.RoleTypeId.FacilityGuard: return Color("Facility Guard", "#5b6370");
                case PlayerRoles.RoleTypeId.NtfPrivate: return Color("NTF Private", "#0096ff");
                case PlayerRoles.RoleTypeId.NtfSergeant: return Color("NTF Sergeant", "#0070dd");
                case PlayerRoles.RoleTypeId.NtfSpecialist: return Color("NTF Specialist", "#003399");
                case PlayerRoles.RoleTypeId.NtfCaptain: return Color("NTF Captain", "#000080");
                case PlayerRoles.RoleTypeId.ChaosConscript: return Color("CI Conscript", "#008f1e");
                case PlayerRoles.RoleTypeId.ChaosRifleman: return Color("CI Rifleman", "#006b15");
                case PlayerRoles.RoleTypeId.ChaosRepressor: return Color("CI Repressor", "#004d10");
                case PlayerRoles.RoleTypeId.ChaosMarauder: return Color("CI Marauder", "#003300");
                case PlayerRoles.RoleTypeId.Scp173: return Color("SCP-173", "#ff0000");
                case PlayerRoles.RoleTypeId.Scp106: return Color("SCP-106", "#960018");
                case PlayerRoles.RoleTypeId.Scp049: return Color("SCP-049", "#770000");
                case PlayerRoles.RoleTypeId.Scp096: return Color("SCP-096", "#ff4444");
                case PlayerRoles.RoleTypeId.Scp939: return Color("SCP-939", "#ff2200");
                case PlayerRoles.RoleTypeId.Scp079: return Color("SCP-079", "#cc0000");
                case PlayerRoles.RoleTypeId.Scp0492: return Color("SCP-049-2", "#880000");
                case PlayerRoles.RoleTypeId.Scp3114: return Color("SCP-3114", "#990033");
                case PlayerRoles.RoleTypeId.Tutorial: return Color("Tutorial", "#ff00ff");
                case PlayerRoles.RoleTypeId.Spectator: return Color("Spectator", "#999999");
                default: return Color(role.ToString(), "#cccccc");
            }
        }
    }
}
