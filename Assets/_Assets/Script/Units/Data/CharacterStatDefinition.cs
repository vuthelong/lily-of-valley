using System;
using LilyOfValley.Units.Stats;
using UnityEngine;

namespace LilyOfValley.Units.Data
{
    [Serializable]
    public struct CharacterStatDefinition
    {
        #region Field

        private const int MinLevel = 1;

        [SerializeField] private StatType type;

        [SerializeField] private float baseValue;

        [SerializeField] private float perLevel;

        [SerializeField, Tooltip("0 means uncapped.")] private float maxValue;

        #endregion

        #region Property

        public StatType Type => this.type;

        public float BaseValue => this.baseValue;

        public float PerLevel => this.perLevel;

        public float MaxValue => this.maxValue;

        #endregion

        #region Method

        public float Evaluate(int level) => this.baseValue + (this.perLevel * (level - MinLevel));

        #endregion
    }
}
