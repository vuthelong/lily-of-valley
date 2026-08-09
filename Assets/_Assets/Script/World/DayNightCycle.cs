using System;
using UnityEngine;

namespace LilyOfValley.World
{
    [DefaultExecutionOrder(-50)]
    public sealed class DayNightCycle : MonoBehaviour
    {
        #region Fields
        private const float SecondsPerMinute = 60f;
        private const float MinutesPerHour = 60f;

        public event Action<float> TimeChanged;
        public event Action<DayPhase> PhaseChanged;
        public event Action<int> DayChanged;

        [SerializeField] private DayNightPreset preset;
        [SerializeField, Min(0.05f)] private float dayLengthMinutes = 8f;
        [SerializeField, Range(0f, DayNightPreset.HoursPerDay)] private float startHour = 7f;
        [SerializeField] private bool paused;

        private float _normalizedTime;
        private DayPhase _phase = DayPhase.Day;
        private int _day = 1;
        #endregion

        #region Properties
        public DayNightPreset Preset => this.preset;
        public float NormalizedTime => this._normalizedTime;
        public float Hour => this._normalizedTime * DayNightPreset.HoursPerDay;
        public int Minute => Mathf.FloorToInt(Hour % 1f * MinutesPerHour);
        public DayPhase Phase => this._phase;
        public int Day => this._day;
        public bool IsPaused => this.paused;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (this.preset == null) Debug.LogWarning($"{nameof(DayNightCycle)}: no {nameof(DayNightPreset)} assigned; lighting will not update.", this);

            this._normalizedTime = Mathf.Repeat(this.startHour, DayNightPreset.HoursPerDay) / DayNightPreset.HoursPerDay;
            this._phase = EvaluatePhase();
        }

        private void Update()
        {
            if (this.paused) return;

            Advance(Time.deltaTime / (this.dayLengthMinutes * SecondsPerMinute));
        }
        #endregion

        #region Public Methods
        public void SetHour(float hour)
        {
            this._normalizedTime = Mathf.Repeat(hour, DayNightPreset.HoursPerDay) / DayNightPreset.HoursPerDay;
            TimeChanged?.Invoke(this._normalizedTime);
            RefreshPhase();
        }

        public void SetPaused(bool value) => this.paused = value;

        public void SetDayLength(float minutes) => this.dayLengthMinutes = Mathf.Max(0.05f, minutes);

        public void SkipHours(float hours) => Advance(hours / DayNightPreset.HoursPerDay);

        [ContextMenu("Jump To Start Hour")]
        public void JumpToStartHour() => SetHour(this.startHour);

        [ContextMenu("Skip One Hour")]
        public void SkipOneHour() => SkipHours(1f);
        #endregion

        #region Private Methods
        private void Advance(float normalizedDelta)
        {
            this._normalizedTime += normalizedDelta;

            while (this._normalizedTime >= 1f)
            {
                this._normalizedTime -= 1f;
                this._day++;
                DayChanged?.Invoke(this._day);
            }

            TimeChanged?.Invoke(this._normalizedTime);
            RefreshPhase();
        }

        private void RefreshPhase()
        {
            var phase = EvaluatePhase();
            if (phase == this._phase) return;

            this._phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        private DayPhase EvaluatePhase()
        {
            if (this.preset == null) return DayPhase.Day;

            return this.preset.EvaluatePhase(Hour);
        }
        #endregion
    }
}
