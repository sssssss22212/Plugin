using System.ComponentModel;

namespace ClassicGrenades
{
    public sealed class Config
    {
        [Description("Включить плагин.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Блокировать урон по тиммейтам от HE-гранаты.")]
        public bool BlockHe { get; set; } = true;

        [Description("Блокировать урон по тиммейтам от SCP-018.")]
        public bool Block018 { get; set; } = true;

        [Description("Владелец гранаты получает урон от своей же гранаты (true = классика).")]
        public bool OwnerSelfDamage { get; set; } = true;

        [Description("Блокировать HE-урон без владельца (граната без owner, например бросавший вышел).")]
        public bool BlockOrphan { get; set; } = false;
    }
}
