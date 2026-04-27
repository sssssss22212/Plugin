namespace ClassicGrenades
{
    /// <summary>
    /// Plugin configuration. Default behaviour reproduces "classic" friendly-fire
    /// rules: HE / SCP-018 explosions never hit teammates, but always hit the
    /// thrower and enemies.
    /// </summary>
    public sealed class Config
    {
        public bool IsEnabled { get; set; } = true;

        /// <summary>Cancel HE-grenade damage between teammates (vanilla = governed by FF).</summary>
        public bool ProtectTeammatesFromHeGrenades { get; set; } = true;

        /// <summary>Cancel SCP-018 ball damage between teammates.</summary>
        public bool ProtectTeammatesFromScp018 { get; set; } = true;

        /// <summary>
        /// If true, the thrower can still damage themselves with their own grenade
        /// (classic behaviour). If false, the thrower is also immune.
        /// </summary>
        public bool ThrowerSelfDamage { get; set; } = true;

        /// <summary>
        /// If true, log every blocked friendly-fire grenade hit to the server console.
        /// </summary>
        public bool LogBlockedHits { get; set; } = false;
    }
}
