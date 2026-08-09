using System;
using System.Collections.Generic;
using UnityEngine;

namespace LilyOfValley.Core
{
    public static class TimeService
    {
        #region Field

        private const float PausedScale = 0f;

        private const float MinScale = 0f;

        private const float DefaultScale = 1f;

        private const float NoHitStop = 0f;

        private const int NoPauseSource = 0;

        private const int FirstPauseSource = 1;

        private const int PauseSourceCapacity = 4;

        private static readonly HashSet<object> PauseSources = new(PauseSourceCapacity);

        private static float _scale = DefaultScale;

        private static float _hitStopRemaining;

        public static event Action<bool> PausedChanged;

        public static event Action<float> ScaleChanged;

        #endregion

        #region Property

        public static float DeltaTime => Time.deltaTime;

        public static float UnscaledDeltaTime => Time.unscaledDeltaTime;

        public static float FixedDeltaTime => Time.fixedDeltaTime;

        public static float Scale => TimeService._scale;

        public static bool IsPaused => TimeService.PauseSources.Count > NoPauseSource;

        public static bool IsHitStopped => TimeService._hitStopRemaining > NoHitStop;

        public static float EffectiveScale => IsPaused || IsHitStopped ? PausedScale : TimeService._scale;

        #endregion

        #region Pause Control

        public static bool Pause(object source)
        {
            if (source == null) return false;
            if (!TimeService.PauseSources.Add(source)) return false;
            if (TimeService.PauseSources.Count != FirstPauseSource) return true;

            Apply();
            PausedChanged?.Invoke(true);

            return true;
        }

        public static bool Resume(object source)
        {
            if (source == null) return false;
            if (!TimeService.PauseSources.Remove(source)) return false;
            if (TimeService.PauseSources.Count > NoPauseSource) return true;

            Apply();
            PausedChanged?.Invoke(false);

            return true;
        }

        public static void ResumeAll()
        {
            if (TimeService.PauseSources.Count == NoPauseSource) return;

            TimeService.PauseSources.Clear();
            Apply();
            PausedChanged?.Invoke(false);
        }

        #endregion

        #region Scale Control

        public static void SetScale(float scale)
        {
            var clamped = Mathf.Max(MinScale, scale);
            if (Mathf.Approximately(clamped, TimeService._scale)) return;

            TimeService._scale = clamped;
            Apply();
            ScaleChanged?.Invoke(clamped);
        }

        public static void HitStop(float unscaledSeconds)
        {
            if (unscaledSeconds <= NoHitStop) return;

            var wasHitStopped = IsHitStopped;
            TimeService._hitStopRemaining = Mathf.Max(TimeService._hitStopRemaining, unscaledSeconds);

            if (wasHitStopped) return;

            Apply();
        }

        #endregion

        #region Method

        public static void Tick(float unscaledDeltaTime)
        {
            if (!IsHitStopped) return;

            TimeService._hitStopRemaining -= unscaledDeltaTime;

            if (IsHitStopped) return;

            TimeService._hitStopRemaining = NoHitStop;
            Apply();
        }

        public static void Clear()
        {
            TimeService.PauseSources.Clear();
            TimeService._scale = DefaultScale;
            TimeService._hitStopRemaining = NoHitStop;
            PausedChanged = null;
            ScaleChanged = null;
            Apply();
        }

        private static void Apply() => Time.timeScale = EffectiveScale;

        #endregion
    }
}
