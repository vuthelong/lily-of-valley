using System;
using UnityEngine;

namespace LilyOfValley.Units.Stats
{
    public sealed class Stat
    {
        #region Fields
        public event Action<Stat> Changed;

        private float _baseValue;
        private float _baseBonus;
        private float _baseMultiply = 1f;
        private float _totalBonus;
        private float _totalMultiply = 1f;
        private float _minValue;
        private float _maxValue = float.MaxValue;
        private float _value;
        #endregion

        #region Properties
        public StatType Type { get; }
        public float BaseValue => this._baseValue;
        public float Value => this._value;
        #endregion

        #region Public Methods
        public Stat(StatType type, float baseValue, float minValue = 0f, float maxValue = float.MaxValue)
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

        /// <summary>Applies a modifier. Pass a negative amount to remove one.</summary>
        public void Modify(float amount, StatModifyType modifyType)
        {
            if (Mathf.Approximately(amount, 0f)) return;

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

        /// <summary>What <see cref="Value"/> would become, without applying the modifier.</summary>
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
            this._baseBonus = 0f;
            this._baseMultiply = 1f;
            this._totalBonus = 0f;
            this._totalMultiply = 1f;
            Recalculate();
        }

        public void ClearSubscribers() => Changed = null;
        #endregion

        #region Private Methods
        private void Recalculate()
        {
            var value = Clamp(Compute(this._baseValue, this._baseBonus, this._baseMultiply, this._totalBonus, this._totalMultiply));
            if (Mathf.Approximately(this._value, value)) return;

            this._value = value;
            Changed?.Invoke(this);
        }

        private float Clamp(float value) => Mathf.Clamp(value, this._minValue, this._maxValue);

        private static float Compute(float baseValue, float baseBonus, float baseMultiply, float totalBonus, float totalMultiply)
        {
            return (((baseValue + baseBonus) * baseMultiply) + totalBonus) * totalMultiply;
        }
        #endregion
    }
}
