using LilyOfValley.Units.Data;
using LilyOfValley.Units.Stats;
using UnityEditor;
using UnityEngine;
using static LilyOfValley.EditorTools.EditorAssetUtility;
using static LilyOfValley.EditorTools.SerializedFieldUtility;

namespace LilyOfValley.EditorTools
{
    public static class CharacterDataFactory
    {
        #region Field

        private const string SettingsFolder = "Assets/_Assets/Settings";
        private const string PlayerDataPath = SettingsFolder + "/PlayerCharacterData.asset";

        private const string CharacterIdFieldName = "characterId";
        private const string DisplayNameFieldName = "displayName";
        private const string MaxLevelFieldName = "maxLevel";
        private const string BaseExperienceFieldName = "baseExperienceToLevel";
        private const string ExperienceGrowthFieldName = "experienceGrowth";
        private const string StatsFieldName = "stats";

        private const string StatTypeFieldName = "type";
        private const string StatBaseValueFieldName = "baseValue";
        private const string StatPerLevelFieldName = "perLevel";
        private const string StatMaxValueFieldName = "maxValue";

        private const int PlayerCharacterId = 1;
        private const string PlayerDisplayName = "Player";
        private const int PlayerMaxLevel = 30;
        private const int PlayerBaseExperience = 100;
        private const float PlayerExperienceGrowth = 1.25f;

        private static readonly StatSeed[] PlayerStats =
        {
            new(StatType.Health, 100f, 12f),
            new(StatType.HealthRegen, 1f, 0.1f),
            new(StatType.Damage, 10f, 1.5f),
            new(StatType.Armor, 5f, 0.8f),
            new(StatType.AttackSpeed, 1f, 0.02f),
            new(StatType.AttackRange, 2f, 0f),
            new(StatType.MoveSpeed, 4.5f, 0.05f),
            new(StatType.CritChance, 0.05f, 0.002f, 1f),
            new(StatType.CritDamage, 0.5f, 0.01f, 5f),
            new(StatType.LifeSteal, 0f, 0.001f, 1f),
            new(StatType.CooldownReduction, 0f, 0.002f, 0.6f)
        };

        #endregion

        #region Method

        public static CharacterData EnsurePlayerData()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CharacterData>(PlayerDataPath);
            if (existing != null) return existing;

            EnsureFolder(SettingsFolder);

            var data = LoadOrCreate<CharacterData>(PlayerDataPath);
            ApplyFields(data, Write);

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log($"{nameof(CharacterDataFactory)}: created '{PlayerDataPath}'.");
            return data;
        }

        private static void Write(SerializedObject serialized)
        {
            SetInt(serialized, CharacterIdFieldName, PlayerCharacterId);
            SetString(serialized, DisplayNameFieldName, PlayerDisplayName);
            SetInt(serialized, MaxLevelFieldName, PlayerMaxLevel);
            SetInt(serialized, BaseExperienceFieldName, PlayerBaseExperience);
            SetFloat(serialized, ExperienceGrowthFieldName, PlayerExperienceGrowth);
            WriteStats(serialized);
        }

        private static void WriteStats(SerializedObject serialized)
        {
            var stats = serialized.FindProperty(StatsFieldName);
            if (stats == null)
            {
                Debug.LogError($"{nameof(CharacterDataFactory)}: field '{StatsFieldName}' not found on {nameof(CharacterData)}.");
                return;
            }

            stats.arraySize = PlayerStats.Length;

            for (var i = 0; i < PlayerStats.Length; i++)
            {
                var seed = PlayerStats[i];
                var element = stats.GetArrayElementAtIndex(i);

                element.FindPropertyRelative(StatTypeFieldName).intValue = (int)seed.Type;
                element.FindPropertyRelative(StatBaseValueFieldName).floatValue = seed.BaseValue;
                element.FindPropertyRelative(StatPerLevelFieldName).floatValue = seed.PerLevel;
                element.FindPropertyRelative(StatMaxValueFieldName).floatValue = seed.MaxValue;
            }
        }

        #endregion

        private readonly struct StatSeed
        {
            #region Property

            public StatType Type { get; }

            public float BaseValue { get; }

            public float PerLevel { get; }

            public float MaxValue { get; }

            #endregion

            #region Method

            public StatSeed(StatType type, float baseValue, float perLevel, float maxValue = 0f)
            {
                Type = type;
                BaseValue = baseValue;
                PerLevel = perLevel;
                MaxValue = maxValue;
            }

            #endregion
        }
    }
}
