using System;
using LilyOfValley.Units.Data;
using LilyOfValley.Units.Stats;
using UnityEngine;

namespace LilyOfValley.Units
{
    /// <summary>
    /// Runtime data of a character: identity, level, stats and vitals. Plain C#, no scene
    /// dependency, so it can be created and unit tested without a GameObject.
    /// </summary>
    public class CharacterModel : IDisposable
    {
        #region Field

        private const float ArmorConstant = 100f;

        private readonly Stat _healthStat;

        public event Action<float, float> HealthChanged;

        public event Action<CharacterModel> Died;

        public event Action<CharacterModel> Revived;

        public event Action<int> LevelChanged;

        #endregion

        #region Property

        public int Uid { get; }

        public CharacterData Data { get; }

        public StatSet Stats { get; }

        public int CharacterId => Data.CharacterId;

        public string DisplayName => Data.DisplayName;

        public int Level { get; private set; }

        public int Experience { get; private set; }

        public int ExperienceToNextLevel => Data.GetExperienceToNextLevel(Level);

        public bool IsMaxLevel => Level >= Data.MaxLevel;

        public float CurrentHealth { get; private set; }

        public float MaxHealth => this._healthStat.Value;

        public float HealthPercent => MaxHealth > 0f ? CurrentHealth / MaxHealth : 0f;

        public bool IsAlive => CurrentHealth > 0f;

        #endregion

        #region Level Progression

        public CharacterModel(CharacterData data, int level, int uid)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Uid = uid;
            Data = data;
            Stats = new StatSet();

            this._healthStat = Stats.Get(StatType.Health);
            this._healthStat.Changed += OnMaxHealthChanged;

            SetLevel(level);
            RestoreFull();
        }

        public void SetLevel(int level)
        {
            var clamped = Data.ClampLevel(level);
            var ratio = MaxHealth > 0f ? HealthPercent : 1f;

            Level = clamped;
            Data.ApplyTo(Stats, clamped);
            SetHealth(MaxHealth * ratio);
            this.LevelChanged?.Invoke(clamped);
        }

        /// <summary>Adds experience and levels up as many times as it covers. Returns true when the level changed.</summary>
        public bool AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel) return false;

            Experience += amount;
            var leveledUp = false;

            while (!IsMaxLevel)
            {
                var required = Data.GetExperienceToNextLevel(Level);
                if (required <= 0 || Experience < required) break;

                Experience -= required;
                SetLevel(Level + 1);
                leveledUp = true;
            }

            if (IsMaxLevel) Experience = 0;

            return leveledUp;
        }

        #endregion

        #region Damage Handling

        /// <summary>Applies armor mitigation and returns the health actually removed.</summary>
        public virtual float TakeDamage(float amount, CharacterModel source = null)
        {
            if (amount <= 0f || !IsAlive) return 0f;

            var mitigated = amount * GetArmorMultiplier();
            var applied = Mathf.Min(mitigated, CurrentHealth);
            SetHealth(CurrentHealth - applied);

            if (source != null) source.ApplyLifeSteal(applied);

            return applied;
        }

        private float GetArmorMultiplier()
        {
            var armor = Mathf.Max(0f, Stats.GetValue(StatType.Armor));

            return ArmorConstant / (ArmorConstant + armor);
        }

        private void ApplyLifeSteal(float damageDealt)
        {
            var lifeSteal = Stats.GetValue(StatType.LifeSteal);
            if (lifeSteal <= 0f) return;

            Heal(damageDealt * lifeSteal);
        }

        #endregion

        #region Health

        public virtual void Heal(float amount)
        {
            if (amount <= 0f || !IsAlive) return;

            SetHealth(CurrentHealth + amount);
        }

        public void RestoreFull() => SetHealth(MaxHealth);

        public void Revive(float healthPercent = 1f)
        {
            if (IsAlive) return;

            SetHealth(MaxHealth * Mathf.Clamp01(healthPercent));
            this.Revived?.Invoke(this);
        }

        private void SetHealth(float value)
        {
            var clamped = Mathf.Clamp(value, 0f, MaxHealth);
            if (Mathf.Approximately(clamped, CurrentHealth)) return;

            var wasAlive = IsAlive;
            CurrentHealth = clamped;
            this.HealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (wasAlive && !IsAlive) this.Died?.Invoke(this);
        }

        private void OnMaxHealthChanged(Stat stat)
        {
            if (CurrentHealth > stat.Value)
            {
                SetHealth(stat.Value);
                return;
            }

            this.HealthChanged?.Invoke(CurrentHealth, stat.Value);
        }

        #endregion

        #region Method

        public void Dispose()
        {
            if (this._healthStat != null) this._healthStat.Changed -= OnMaxHealthChanged;

            Stats.ClearSubscribers();
            this.HealthChanged = null;
            this.Died = null;
            this.Revived = null;
            this.LevelChanged = null;
        }

        #endregion
    }
}
