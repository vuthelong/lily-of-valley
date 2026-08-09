namespace LilyOfValley.Units.Stats
{
    public sealed class StatSet
    {
        #region Field

        private const int FirstValidIndex = 1;
        private const float DefaultBaseValue = 0f;
        private const float MissingStatValue = 0f;
        private const float UncappedMaxValue = 0f;
        private const float MinLimit = 0f;
        private const float NoMaxLimit = float.MaxValue;

        private readonly Stat[] _stats = new Stat[(int)StatType.Count];

        #endregion

        #region Lookup

        public Stat Get(StatType statType)
        {
            var index = (int)statType;
            if (!IsValidIndex(index)) return null;

            return this._stats[index] ??= new Stat(statType, DefaultBaseValue);
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
            if (!IsValidIndex(index)) return MissingStatValue;

            var stat = this._stats[index];

            return stat != null ? stat.Value : MissingStatValue;
        }

        #endregion

        #region Modification

        public Stat SetBase(StatType statType, float baseValue, float maxValue = UncappedMaxValue)
        {
            var stat = Get(statType);
            if (stat == null) return null;

            stat.SetLimits(MinLimit, maxValue > UncappedMaxValue ? maxValue : NoMaxLimit);
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

        #endregion

        #region Method

        public void ClearSubscribers()
        {
            for (var i = 0; i < this._stats.Length; i++)
            {
                if (this._stats[i] == null) continue;

                this._stats[i].ClearSubscribers();
            }
        }

        private bool IsValidIndex(int index) => index >= FirstValidIndex && index < this._stats.Length;

        #endregion
    }
}
