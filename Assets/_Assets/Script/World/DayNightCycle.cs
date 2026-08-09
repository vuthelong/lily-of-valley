using System;
using UnityEngine;

namespace LilyOfValley.World
{
    [DefaultExecutionOrder(ExecutionOrder)]
    public sealed class DayNightCycle : MonoBehaviour
    {
        #region Field

        private const int ExecutionOrder = -50;
        private const float SecondsPerMinute = 60f;
        private const float MinutesPerHour = 60f;
        private const float MinDayLengthMinutes = 0.05f;
        private const float FullCycle = 1f;
        private const float OneHour = 1f;
        private const int FirstDay = 1;

        private const string JumpToStartHourMenu = "Jump To Start Hour";
        private const string SkipOneHourMenu = "Skip One Hour";

        [SerializeField] private DayNightPreset preset;

        [SerializeField, Min(MinDayLengthMinutes)] private float dayLengthMinutes = 8f;

        [SerializeField, Range(0f, DayNightPreset.HoursPerDay)] private float startHour = 7f;

        [SerializeField] private bool paused;

        private float _normalizedTime;
        private DayPhase _phase = DayPhase.Day;
        private int _day = FirstDay;

        public event Action<float> TimeChanged;
        public event Action<DayPhase> PhaseChanged;
        public event Action<int> DayChanged;

        #endregion

        #region Property

        public DayNightPreset Preset => this.preset;

        public float NormalizedTime => this._normalizedTime;

        public float Hour => this._normalizedTime * DayNightPreset.HoursPerDay;

        public int Minute => Mathf.FloorToInt(Hour % OneHour * MinutesPerHour);

        public DayPhase Phase => this._phase;

        public int Day => this._day;

        public bool IsPaused => this.paused;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (this.preset == null)
            {
                Debug.LogWarning($"{nameof(DayNightCycle)}: no {nameof(DayNightPreset)} assigned; lighting will not update.", this);
            }

            this._normalizedTime = Mathf.Repeat(this.startHour, DayNightPreset.HoursPerDay) / DayNightPreset.HoursPerDay;
            this._phase = EvaluatePhase();
        }

        private void Update()
        {
            if (this.paused) return;

            Advance(Time.deltaTime / (this.dayLengthMinutes * SecondsPerMinute));
        }

        #endregion

        #region Time Control

        public void SetHour(float hour)
        {
            this._normalizedTime = Mathf.Repeat(hour, DayNightPreset.HoursPerDay) / DayNightPreset.HoursPerDay;
            TimeChanged?.Invoke(this._normalizedTime);
            RefreshPhase();
        }

        public void SkipHours(float hours) => Advance(hours / DayNightPreset.HoursPerDay);

        [ContextMenu(JumpToStartHourMenu)]
        public void JumpToStartHour() => SetHour(this.startHour);

        [ContextMenu(SkipOneHourMenu)]
        public void SkipOneHour() => SkipHours(OneHour);

        public void SetPaused(bool isPaused) => this.paused = isPaused;

        public void SetDayLength(float minutes) => this.dayLengthMinutes = Mathf.Max(MinDayLengthMinutes, minutes);

        private void Advance(float normalizedDelta)
        {
            this._normalizedTime += normalizedDelta;

            while (this._normalizedTime >= FullCycle)
            {
                this._normalizedTime -= FullCycle;
                this._day++;
                DayChanged?.Invoke(this._day);
            }

            TimeChanged?.Invoke(this._normalizedTime);
            RefreshPhase();
        }

        #endregion

        #region Phase Tracking

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
