using LilyOfValley.Units.Stats;
using UnityEngine;

namespace LilyOfValley.Units.Behaviours
{
    /// <summary>Reference implementation of a ticking behaviour: heals by the HealthRegen stat.</summary>
    public sealed class CharacterRegenBehaviour : CharacterBehaviour, ICharacterTick
    {
        #region Fields
        [SerializeField, Min(0.05f)] private float tickInterval = 1f;

        private float _timer;
        #endregion

        #region Public Methods
        public void Tick(float deltaTime)
        {
            if (!IsAttached || !Model.IsAlive) return;

            this._timer += deltaTime;
            if (this._timer < this.tickInterval) return;

            var regen = Model.Stats.GetValue(StatType.HealthRegen);
            if (regen > 0f) Model.Heal(regen * this._timer);

            this._timer = 0f;
        }
        #endregion

        #region Private Methods
        protected override void OnAttached() => this._timer = 0f;
        #endregion
    }
}
