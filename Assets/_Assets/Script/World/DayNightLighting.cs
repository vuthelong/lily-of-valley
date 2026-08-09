using UnityEngine;
using UnityEngine.Rendering;

namespace LilyOfValley.World
{
    public sealed class DayNightLighting : MonoBehaviour
    {
        #region Fields
        private const float IntensityEpsilon = 0.001f;

        [SerializeField] private DayNightCycle cycle;
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;
        [SerializeField, Range(0f, 360f)] private float orbitYaw = 170f;
        [SerializeField] private bool driveAmbient = true;
        [SerializeField] private bool driveFog = true;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (this.cycle != null && this.sunLight != null) return;

            Debug.LogError($"{nameof(DayNightLighting)}: {nameof(this.cycle)} and {nameof(this.sunLight)} must be assigned.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (this.cycle == null) return;

            this.cycle.TimeChanged += OnTimeChanged;
        }

        private void Start() => Apply(this.cycle.NormalizedTime);

        private void OnDisable()
        {
            if (this.cycle == null) return;

            this.cycle.TimeChanged -= OnTimeChanged;
        }
        #endregion

        #region Private Methods
        private void Apply(float normalizedTime)
        {
            var preset = this.cycle.Preset;
            if (preset == null) return;

            ApplySun(preset, normalizedTime);
            ApplyMoon(preset, normalizedTime);

            if (this.driveAmbient)
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = preset.EvaluateAmbientColor(normalizedTime);
            }

            if (!this.driveFog) return;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = preset.EvaluateFogColor(normalizedTime);
            RenderSettings.fogDensity = preset.EvaluateFogDensity(normalizedTime);
        }

        private void ApplySun(DayNightPreset preset, float normalizedTime)
        {
            var elevation = (normalizedTime * 360f) - 90f;
            this.sunLight.transform.rotation = Quaternion.Euler(elevation, this.orbitYaw, 0f);
            this.sunLight.color = preset.EvaluateSunColor(normalizedTime);

            var intensity = preset.EvaluateSunIntensity(normalizedTime);
            this.sunLight.intensity = intensity;
            this.sunLight.enabled = intensity > IntensityEpsilon;
        }

        private void ApplyMoon(DayNightPreset preset, float normalizedTime)
        {
            if (this.moonLight == null) return;

            var elevation = (normalizedTime * 360f) + 90f;
            this.moonLight.transform.rotation = Quaternion.Euler(elevation, this.orbitYaw, 0f);
            this.moonLight.color = preset.EvaluateMoonColor(normalizedTime);

            var intensity = preset.EvaluateMoonIntensity(normalizedTime);
            this.moonLight.intensity = intensity;
            this.moonLight.enabled = intensity > IntensityEpsilon;
        }
        #endregion

        #region Unity Callbacks
        private void OnTimeChanged(float normalizedTime) => Apply(normalizedTime);
        #endregion
    }
}
