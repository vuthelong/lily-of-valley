namespace LilyOfValley.Units.Stats
{
    public enum StatType
    {
        None = 0,
        Health = 1,
        HealthRegen = 2,
        Damage = 3,
        Armor = 4,
        AttackSpeed = 5,
        AttackRange = 6,
        MoveSpeed = 7,
        CritChance = 8,
        CritDamage = 9,
        LifeSteal = 10,
        CooldownReduction = 11,

        // Keep last: sizes the stat array.
        Count = 12
    }

    public static class StatTypeExtensions
    {
        #region Public Methods
        public static bool IsPercent(this StatType statType) => statType switch
        {
            StatType.CritChance => true,
            StatType.CritDamage => true,
            StatType.LifeSteal => true,
            StatType.CooldownReduction => true,
            _ => false
        };
        #endregion
    }
}
