using LilyOfValley.Units.Stats;
using UnityEngine;

namespace LilyOfValley.Units.Behaviours
{
    public sealed class CharacterRegenBehaviour : CharacterBehaviour, ICharacterTick
    {
        #region Field

        private const float MinTickInterval = 0.05f;

        [SerializeField, Min(MinTickInterval)] private float tickInterval = 1f;

        private float _timer;

        #endregion

        #region Method

        public void Tick(float deltaTime)
        {
            if (!IsAttached || !Model.IsAlive) return;

            this._timer += deltaTime;
            if (this._timer < this.tickInterval) return;

            var regen = Model.Stats.GetValue(StatType.HealthRegen);
            if (regen > 0f) Model.Heal(regen * this._timer);

            this._timer = 0f;
        }

        protected override void OnAttached() => this._timer = 0f;

        #endregion
    }
}
