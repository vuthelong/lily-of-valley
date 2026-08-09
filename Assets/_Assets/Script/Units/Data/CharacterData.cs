using System.Collections.Generic;
using LilyOfValley.Units.Stats;
using UnityEngine;

namespace LilyOfValley.Units.Data
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Lily of Valley/Units/Character Data")]
    public sealed class CharacterData : ScriptableObject
    {
        #region Field

        private const int MinLevel = 1;

        [SerializeField] private int characterId;

        [SerializeField] private string displayName = "Character";

        [SerializeField] private Character prefab;

        [Header("Level")]
        [SerializeField, Min(1)] private int maxLevel = 30;

        [SerializeField, Min(1)] private int baseExperienceToLevel = 100;

        [SerializeField, Min(1f)] private float experienceGrowth = 1.25f;

        [Header("Stats")]
        [SerializeField] private CharacterStatDefinition[] stats;

        #endregion

        #region Property

        public int CharacterId => this.characterId;

        public string DisplayName => this.displayName;

        public Character Prefab => this.prefab;

        public int MaxLevel => this.maxLevel;

        public IReadOnlyList<CharacterStatDefinition> Stats => this.stats;

        #endregion

        #region Level Progression

        public int ClampLevel(int level) => Mathf.Clamp(level, MinLevel, this.maxLevel);

        public int GetExperienceToNextLevel(int level)
        {
            if (level >= this.maxLevel) return 0;

            var steps = Mathf.Max(0, level - MinLevel);
            return Mathf.CeilToInt(this.baseExperienceToLevel * Mathf.Pow(this.experienceGrowth, steps));
        }

        #endregion

        #region Stat Evaluation

        public float GetStatValue(StatType statType, int level)
        {
            if (this.stats == null) return 0f;

            for (var i = 0; i < this.stats.Length; i++)
            {
                if (this.stats[i].Type != statType) continue;

                return this.stats[i].Evaluate(level);
            }

            return 0f;
        }

        public void ApplyTo(StatSet statSet, int level)
        {
            if (statSet == null || this.stats == null) return;

            for (var i = 0; i < this.stats.Length; i++)
            {
                var definition = this.stats[i];
                if (definition.Type == StatType.None) continue;

                statSet.SetBase(definition.Type, definition.Evaluate(level), definition.MaxValue);
            }
        }

        #endregion
    }
}
