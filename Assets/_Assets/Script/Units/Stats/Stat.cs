using System;
using UnityEngine;

namespace LilyOfValley.Units.Stats
{
    public sealed class Stat
    {
        #region Field

        private const float NoModifierAmount = 0f;
        private const float NeutralBonus = 0f;
        private const float NeutralMultiplier = 1f;
        private const float DefaultMinValue = 0f;
        private const float UnlimitedMaxValue = float.MaxValue;

        private float _baseValue;
        private float _baseBonus;
        private float _baseMultiply = NeutralMultiplier;
        private float _totalBonus;
        private float _totalMultiply = NeutralMultiplier;
        private float _minValue;
        private float _maxValue = UnlimitedMaxValue;
        private float _value;

        public event Action<Stat> Changed;

        #endregion

        #region Property

        public StatType Type { get; }

        public float BaseValue => this._baseValue;

        public float Value => this._value;

        #endregion

        #region Configuration

        public Stat(StatType type, float baseValue, float minValue = DefaultMinValue, float maxValue = UnlimitedMaxValue)
        {
            Type = type;
            this._baseValue = baseValue;
            this._minValue = minValue;
            this._maxValue = maxValue;
            Recalculate();
        }

        public void SetBaseValue(float value)
        {
            if (Mathf.Approximately(this._baseValue, value)) return;

            this._baseValue = value;
            Recalculate();
        }

        public void SetLimits(float minValue, float maxValue)
        {
            this._minValue = minValue;
            this._maxValue = Mathf.Max(minValue, maxValue);
            Recalculate();
        }

        #endregion

        #region Modification

        public void Modify(float amount, StatModifyType modifyType)
        {
            if (Mathf.Approximately(amount, NoModifierAmount)) return;

            switch (modifyType)
            {
                case StatModifyType.BaseBonus:
                    this._baseBonus += amount;
                    break;

                case StatModifyType.BaseMultiply:
                    this._baseMultiply += amount;
                    break;

                case StatModifyType.TotalBonus:
                    this._totalBonus += amount;
                    break;

                case StatModifyType.TotalMultiply:
                    this._totalMultiply += amount;
                    break;
            }

            Recalculate();
        }

        public float Preview(float amount, StatModifyType modifyType)
        {
            var baseBonus = this._baseBonus;
            var baseMultiply = this._baseMultiply;
            var totalBonus = this._totalBonus;
            var totalMultiply = this._totalMultiply;

            switch (modifyType)
            {
                case StatModifyType.BaseBonus:
                    baseBonus += amount;
                    break;

                case StatModifyType.BaseMultiply:
                    baseMultiply += amount;
                    break;

                case StatModifyType.TotalBonus:
                    totalBonus += amount;
                    break;

                case StatModifyType.TotalMultiply:
                    totalMultiply += amount;
                    break;
            }

            return Clamp(Compute(this._baseValue, baseBonus, baseMultiply, totalBonus, totalMultiply));
        }

        public void ResetModifiers()
        {
            this._baseBonus = NeutralBonus;
            this._baseMultiply = NeutralMultiplier;
            this._totalBonus = NeutralBonus;
            this._totalMultiply = NeutralMultiplier;
            Recalculate();
        }

        #endregion

        #region Calculation

        private void Recalculate()
        {
            var value = Clamp(Compute(this._baseValue, this._baseBonus, this._baseMultiply, this._totalBonus, this._totalMultiply));
            if (Mathf.Approximately(this._value, value)) return;

            this._value = value;
            this.Changed?.Invoke(this);
        }

        private float Clamp(float value) => Mathf.Clamp(value, this._minValue, this._maxValue);

        private static float Compute(float baseValue, float baseBonus, float baseMultiply, float totalBonus, float totalMultiply)
        {
            return (((baseValue + baseBonus) * baseMultiply) + totalBonus) * totalMultiply;
        }

        #endregion

        #region Method

        public void ClearSubscribers() => this.Changed = null;

        #endregion
    }
}
