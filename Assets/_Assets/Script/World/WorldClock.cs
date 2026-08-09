using System;
using UnityEngine;

namespace LilyOfValley.World
{
    public static class WorldClock
    {
        #region Field

        public const float HoursPerDay = 24f;

        public const int SeasonCount = 4;

        private const int DefaultDaysPerSeason = 28;

        private const int MinDaysPerSeason = 1;

        private const int FirstDay = 1;

        private const int FirstYear = 1;

        private const int NoDays = 0;

        private const float FullCycle = 1f;

        private const float DayStart = 0f;

        private const float NoDelta = 0f;

        private const float MinutesPerHour = 60f;

        private const float OneHour = 1f;

        private static int _daysPerSeason = DefaultDaysPerSeason;

        private static int _totalDays;

        private static float _timeOfDay;

        public static event Action<float> TimeChanged;

        public static event Action<int> DayChanged;

        public static event Action<Season> SeasonChanged;

        public static event Action<int> YearChanged;

        #endregion

        #region Property

        public static float NormalizedTime => WorldClock._timeOfDay;

        public static float Hour => WorldClock._timeOfDay * HoursPerDay;

        public static int Minute => Mathf.FloorToInt(Hour % OneHour * MinutesPerHour);

        public static int TotalDays => WorldClock._totalDays;

        public static int Day => WorldClock._totalDays + FirstDay;

        public static int DayOfSeason => (WorldClock._totalDays % WorldClock._daysPerSeason) + FirstDay;

        public static Season CurrentSeason => (Season)(WorldClock._totalDays / WorldClock._daysPerSeason % SeasonCount);

        public static int Year => (WorldClock._totalDays / (WorldClock._daysPerSeason * SeasonCount)) + FirstYear;

        public static int DaysPerSeason => WorldClock._daysPerSeason;

        #endregion

        #region Time Control

        public static void Advance(float normalizedDelta)
        {
            if (normalizedDelta <= NoDelta) return;

            WorldClock._timeOfDay += normalizedDelta;

            if (WorldClock._timeOfDay < FullCycle)
            {
                TimeChanged?.Invoke(WorldClock._timeOfDay);
                return;
            }

            var previousSeason = CurrentSeason;
            var previousYear = Year;

            while (WorldClock._timeOfDay >= FullCycle)
            {
                WorldClock._timeOfDay -= FullCycle;
                WorldClock._totalDays++;
                DayChanged?.Invoke(Day);
            }

            TimeChanged?.Invoke(WorldClock._timeOfDay);
            RaiseCalendarChanges(previousSeason, previousYear);
        }

        public static void SetTimeOfDay(float normalizedTime)
        {
            WorldClock._timeOfDay = Mathf.Repeat(normalizedTime, FullCycle);
            TimeChanged?.Invoke(WorldClock._timeOfDay);
        }

        public static void SetHour(float hour) => SetTimeOfDay(Mathf.Repeat(hour, HoursPerDay) / HoursPerDay);

        public static void SkipHours(float hours) => Advance(hours / HoursPerDay);

        #endregion

        #region Persistence

        public static WorldClockState Capture() =>
            new() { totalDays = WorldClock._totalDays, timeOfDay = WorldClock._timeOfDay };

        public static void Restore(WorldClockState state)
        {
            var previousSeason = CurrentSeason;
            var previousYear = Year;

            WorldClock._totalDays = Mathf.Max(NoDays, state.totalDays);
            WorldClock._timeOfDay = Mathf.Repeat(state.timeOfDay, FullCycle);

            TimeChanged?.Invoke(WorldClock._timeOfDay);
            DayChanged?.Invoke(Day);
            RaiseCalendarChanges(previousSeason, previousYear);
        }

        #endregion

        #region Method

        public static void Configure(int daysPerSeason) =>
            WorldClock._daysPerSeason = Mathf.Max(MinDaysPerSeason, daysPerSeason);

        public static void Clear()
        {
            WorldClock._daysPerSeason = DefaultDaysPerSeason;
            WorldClock._totalDays = NoDays;
            WorldClock._timeOfDay = DayStart;
            TimeChanged = null;
            DayChanged = null;
            SeasonChanged = null;
            YearChanged = null;
        }

        private static void RaiseCalendarChanges(Season previousSeason, int previousYear)
        {
            if (CurrentSeason != previousSeason) SeasonChanged?.Invoke(CurrentSeason);

            if (Year != previousYear) YearChanged?.Invoke(Year);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() => Clear();

        #endregion
    }
}
