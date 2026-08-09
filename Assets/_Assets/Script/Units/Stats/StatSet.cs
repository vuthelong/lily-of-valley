namespace LilyOfValley.Units.Stats
{
    /// <summary>
    /// Stats indexed by <see cref="StatType"/>. Flat array, so lookups are O(1) with no hashing
    /// and no allocation for stats that were never configured.
    /// </summary>
    public sealed class StatSet
    {
        #region Fields
        private readonly Stat[] _stats = new Stat[(int)StatType.Count];
        #endregion

        #region Public Methods
        /// <summary>Returns the stat, creating an empty one when it was never configured.</summary>
        public Stat Get(StatType statType)
        {
            var index = (int)statType;
            if (!IsValidIndex(index)) return null;

            return this._stats[index] ??= new Stat(statType, 0f);
        }

        public bool TryGet(StatType statType, out Stat stat)
        {
            var index = (int)statType;
            stat = IsValidIndex(index) ? this._stats[index] : null;
            return stat != null;
        }

        public float GetValue(StatType statType)
        {
            var index = (int)statType;
            if (!IsValidIndex(index)) return 0f;

            var stat = this._stats[index];
            return stat != null ? stat.Value : 0f;
        }

        /// <summary>Sets the base value and limits, keeping any modifiers already applied.</summary>
        public Stat SetBase(StatType statType, float baseValue, float maxValue = 0f)
        {
            var stat = Get(statType);
            if (stat == null) return null;

            stat.SetLimits(0f, maxValue > 0f ? maxValue : float.MaxValue);
            stat.SetBaseValue(baseValue);
            return stat;
        }

        public void Modify(StatType statType, float amount, StatModifyType modifyType)
        {
            var stat = Get(statType);
            if (stat == null) return;

            stat.Modify(amount, modifyType);
        }

        public void ResetModifiers()
        {
            for (var i = 0; i < this._stats.Length; i++)
            {
                if (this._stats[i] == null) continue;

                this._stats[i].ResetModifiers();
            }
        }

        public void ClearSubscribers()
        {
            for (var i = 0; i < this._stats.Length; i++)
            {
                if (this._stats[i] == null) continue;

                this._stats[i].ClearSubscribers();
            }
        }
        #endregion

        #region Private Methods
        private bool IsValidIndex(int index) => index > 0 && index < this._stats.Length;
        #endregion
    }
}
