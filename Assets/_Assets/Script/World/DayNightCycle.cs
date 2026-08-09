using System;
using LilyOfValley.Core.Updates;
using UnityEngine;

namespace LilyOfValley.World
{
    [DefaultExecutionOrder(ExecutionOrder)]
    public sealed class DayNightCycle : MonoBehaviour, IUpdatable
    {
        #region Field

        private const int ExecutionOrder = -50;
        private const float SecondsPerMinute = 60f;
        private const float MinDayLengthMinutes = 0.05f;
        private const int MinDaysPerSeason = 1;
        private const float OneHour = 1f;

        private const string JumpToStartHourMenu = "Jump To Start Hour";
        private const string SkipOneHourMenu = "Skip One Hour";

        [SerializeField] private DayNightPreset preset;

        [SerializeField, Min(MinDayLengthMinutes)] private float dayLengthMinutes = 8f;

        [SerializeField, Range(0f, WorldClock.HoursPerDay)] private float startHour = 7f;

        [SerializeField, Min(MinDaysPerSeason)] private int daysPerSeason = 28;

        [SerializeField] private bool paused;

        private DayPhase _phase = DayPhase.Day;

        public event Action<float> TimeChanged;
        public event Action<DayPhase> PhaseChanged;
        public event Action<int> DayChanged;

        #endregion

        #region Property

        public DayNightPreset Preset => this.preset;

        public float NormalizedTime => WorldClock.NormalizedTime;

        public float Hour => WorldClock.Hour;

        public int Minute => WorldClock.Minute;

        public DayPhase Phase => this._phase;

        public int Day => WorldClock.Day;

        public int DayOfSeason => WorldClock.DayOfSeason;

        public Season Season => WorldClock.CurrentSeason;

        public int Year => WorldClock.Year;

        public bool IsPaused => this.paused;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (this.preset == null)
            {
                Debug.LogWarning($"{nameof(DayNightCycle)}: no {nameof(DayNightPreset)} assigned; lighting will not update.", this);
            }

            WorldClock.Configure(this.daysPerSeason);
            WorldClock.SetHour(this.startHour);
            this._phase = EvaluatePhase();
        }

        private void OnEnable()
        {
            WorldClock.TimeChanged += OnWorldTimeChanged;
            WorldClock.DayChanged += OnWorldDayChanged;
            UpdateManager.Register(this);
        }

        private void OnDisable()
        {
            UpdateManager.Unregister(this);
            WorldClock.TimeChanged -= OnWorldTimeChanged;
            WorldClock.DayChanged -= OnWorldDayChanged;
        }

        #endregion

        #region Time Control

        public void UpdateManually(float deltaTime)
        {
            if (this.paused) return;

            WorldClock.Advance(deltaTime / (this.dayLengthMinutes * SecondsPerMinute));
        }

        public void SetHour(float hour) => WorldClock.SetHour(hour);

        public void SkipHours(float hours) => WorldClock.SkipHours(hours);

        [ContextMenu(JumpToStartHourMenu)]
        public void JumpToStartHour() => SetHour(this.startHour);

        [ContextMenu(SkipOneHourMenu)]
        public void SkipOneHour() => SkipHours(OneHour);

        public void SetPaused(bool isPaused) => this.paused = isPaused;

        public void SetDayLength(float minutes) => this.dayLengthMinutes = Mathf.Max(MinDayLengthMinutes, minutes);

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

        #region Method

        private void OnWorldTimeChanged(float normalizedTime)
        {
            TimeChanged?.Invoke(normalizedTime);
            RefreshPhase();
        }

        private void OnWorldDayChanged(int day) => DayChanged?.Invoke(day);

        #endregion
    }
}
