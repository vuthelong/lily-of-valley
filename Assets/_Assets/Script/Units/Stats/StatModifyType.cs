namespace LilyOfValley.Units.Stats
{
    /// <summary>
    /// Where a modifier lands in the stat formula:
    /// ((BaseValue + BaseBonus) * BaseMultiply + TotalBonus) * TotalMultiply
    /// </summary>
    public enum StatModifyType
    {
        BaseBonus = 0,
        BaseMultiply = 1,
        TotalBonus = 2,
        TotalMultiply = 3
    }
}
