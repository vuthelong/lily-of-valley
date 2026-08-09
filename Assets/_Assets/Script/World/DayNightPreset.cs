using UnityEngine;

namespace LilyOfValley.World
{
    [CreateAssetMenu(fileName = "DayNightPreset", menuName = "Lily of Valley/World/Day Night Preset")]
    public sealed class DayNightPreset : ScriptableObject
    {
        #region Fields
        public const float HoursPerDay = 24f;

        [Header("Sun")]
        [SerializeField] private Gradient sunColor = new();
        [SerializeField] private AnimationCurve sunIntensity = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Header("Moon")]
        [SerializeField] private Gradient moonColor = new();
        [SerializeField] private AnimationCurve moonIntensity = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        [Header("Environment")]
        [SerializeField] private Gradient ambientColor = new();
        [SerializeField] private Gradient fogColor = new();
        [SerializeField] private AnimationCurve fogDensity = AnimationCurve.Linear(0f, 0.005f, 1f, 0.005f);
        [SerializeField] private AnimationCurve skyBlend = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve skyExposure = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Header("Phase boundaries (hours)")]
        [SerializeField, Range(0f, HoursPerDay)] private float dawnStartHour = 5f;
        [SerializeField, Range(0f, HoursPerDay)] private float dayStartHour = 8f;
        [SerializeField, Range(0f, HoursPerDay)] private float duskStartHour = 17f;
        [SerializeField, Range(0f, HoursPerDay)] private float nightStartHour = 20f;
        #endregion

        #region Public Methods
        public Color EvaluateSunColor(float normalizedTime) => this.sunColor.Evaluate(normalizedTime);

        public float EvaluateSunIntensity(float normalizedTime) => Mathf.Max(0f, this.sunIntensity.Evaluate(normalizedTime));

        public Color EvaluateMoonColor(float normalizedTime) => this.moonColor.Evaluate(normalizedTime);

        public float EvaluateMoonIntensity(float normalizedTime) => Mathf.Max(0f, this.moonIntensity.Evaluate(normalizedTime));

        public Color EvaluateAmbientColor(float normalizedTime) => this.ambientColor.Evaluate(normalizedTime);

        public Color EvaluateFogColor(float normalizedTime) => this.fogColor.Evaluate(normalizedTime);

        public float EvaluateFogDensity(float normalizedTime) => Mathf.Max(0f, this.fogDensity.Evaluate(normalizedTime));

        public float EvaluateSkyBlend(float normalizedTime) => Mathf.Clamp01(this.skyBlend.Evaluate(normalizedTime));

        public float EvaluateSkyExposure(float normalizedTime) => Mathf.Max(0f, this.skyExposure.Evaluate(normalizedTime));

        public DayPhase EvaluatePhase(float hour)
        {
            if (hour >= this.nightStartHour || hour < this.dawnStartHour) return DayPhase.Night;
            if (hour < this.dayStartHour) return DayPhase.Dawn;
            if (hour < this.duskStartHour) return DayPhase.Day;

            return DayPhase.Dusk;
        }

        [ContextMenu("Apply Defaults")]
        public void ApplyDefaults()
        {
            this.sunColor = CreateGradient(
                new[] { 0f, 0.22f, 0.28f, 0.5f, 0.72f, 0.78f, 1f },
                new[]
                {
                    new Color(0.10f, 0.13f, 0.25f),
                    new Color(0.20f, 0.20f, 0.32f),
                    new Color(1.00f, 0.62f, 0.35f),
                    new Color(1.00f, 0.96f, 0.88f),
                    new Color(1.00f, 0.55f, 0.30f),
                    new Color(0.22f, 0.20f, 0.34f),
                    new Color(0.10f, 0.13f, 0.25f)
                });

            this.sunIntensity = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.24f, 0f),
                new Keyframe(0.30f, 0.9f),
                new Keyframe(0.5f, 1.35f),
                new Keyframe(0.70f, 0.9f),
                new Keyframe(0.76f, 0f),
                new Keyframe(1f, 0f));

            this.moonColor = CreateGradient(
                new[] { 0f, 1f },
                new[] { new Color(0.55f, 0.65f, 0.95f), new Color(0.55f, 0.65f, 0.95f) });

            this.moonIntensity = new AnimationCurve(
                new Keyframe(0f, 0.35f),
                new Keyframe(0.22f, 0.30f),
                new Keyframe(0.30f, 0f),
                new Keyframe(0.70f, 0f),
                new Keyframe(0.78f, 0.30f),
                new Keyframe(1f, 0.35f));

            this.ambientColor = CreateGradient(
                new[] { 0f, 0.25f, 0.35f, 0.5f, 0.65f, 0.75f, 1f },
                new[]
                {
                    new Color(0.06f, 0.07f, 0.14f),
                    new Color(0.15f, 0.14f, 0.22f),
                    new Color(0.42f, 0.44f, 0.50f),
                    new Color(0.55f, 0.57f, 0.62f),
                    new Color(0.42f, 0.40f, 0.44f),
                    new Color(0.15f, 0.14f, 0.22f),
                    new Color(0.06f, 0.07f, 0.14f)
                });

            this.fogColor = CreateGradient(
                new[] { 0f, 0.27f, 0.5f, 0.73f, 1f },
                new[]
                {
                    new Color(0.05f, 0.06f, 0.12f),
                    new Color(0.72f, 0.52f, 0.42f),
                    new Color(0.70f, 0.78f, 0.88f),
                    new Color(0.72f, 0.48f, 0.38f),
                    new Color(0.05f, 0.06f, 0.12f)
                });

            this.fogDensity = new AnimationCurve(
                new Keyframe(0f, 0.010f),
                new Keyframe(0.27f, 0.016f),
                new Keyframe(0.5f, 0.004f),
                new Keyframe(0.73f, 0.016f),
                new Keyframe(1f, 0.010f));

            this.skyBlend = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.22f, 0f),
                new Keyframe(0.32f, 1f),
                new Keyframe(0.68f, 1f),
                new Keyframe(0.78f, 0f),
                new Keyframe(1f, 0f));

            this.skyExposure = new AnimationCurve(
                new Keyframe(0f, 0.55f),
                new Keyframe(0.25f, 0.8f),
                new Keyframe(0.5f, 1.15f),
                new Keyframe(0.75f, 0.8f),
                new Keyframe(1f, 0.55f));
        }
        #endregion

        #region Private Methods
        private static Gradient CreateGradient(float[] times, Color[] colors)
        {
            var gradient = new Gradient();
            var colorKeys = new GradientColorKey[colors.Length];
            for (var i = 0; i < colors.Length; i++) colorKeys[i] = new GradientColorKey(colors[i], times[i]);

            var alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }
        #endregion

        #region Unity Callbacks
        private void OnValidate()
        {
            this.dayStartHour = Mathf.Max(this.dayStartHour, this.dawnStartHour);
            this.duskStartHour = Mathf.Max(this.duskStartHour, this.dayStartHour);
            this.nightStartHour = Mathf.Max(this.nightStartHour, this.duskStartHour);
        }
        #endregion
    }
}
