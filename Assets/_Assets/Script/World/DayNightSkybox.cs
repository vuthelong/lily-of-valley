using UnityEngine;

namespace LilyOfValley.World
{
    public sealed class DayNightSkybox : MonoBehaviour
    {
        #region Fields
        private static readonly int BlendId = Shader.PropertyToID("_Blend");
        private static readonly int ExposureId = Shader.PropertyToID("_Exposure");
        private static readonly int RotationId = Shader.PropertyToID("_Rotation");

        [SerializeField] private DayNightCycle cycle;
        [SerializeField] private Material skyboxTemplate;
        [SerializeField] private float rotationDegreesPerSecond = 0.4f;
        [SerializeField, Min(0f)] private float environmentRefreshSeconds = 2f;

        private Material _runtimeSkybox;
        private Material _previousSkybox;
        private float _rotation;
        private float _refreshTimer;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (this.cycle == null || this.skyboxTemplate == null)
            {
                Debug.LogError($"{nameof(DayNightSkybox)}: {nameof(this.cycle)} and {nameof(this.skyboxTemplate)} must be assigned.", this);
                enabled = false;
                return;
            }

            this._previousSkybox = RenderSettings.skybox;
            this._runtimeSkybox = new Material(this.skyboxTemplate);
            RenderSettings.skybox = this._runtimeSkybox;
        }

        private void OnEnable()
        {
            if (this.cycle == null) return;

            this.cycle.TimeChanged += OnTimeChanged;
        }

        private void Start() => Apply(this.cycle.NormalizedTime);

        private void Update()
        {
            this._rotation = Mathf.Repeat(this._rotation + (this.rotationDegreesPerSecond * Time.deltaTime), 360f);
            this._runtimeSkybox.SetFloat(RotationId, this._rotation);

            if (this.environmentRefreshSeconds <= 0f) return;

            this._refreshTimer += Time.deltaTime;
            if (this._refreshTimer < this.environmentRefreshSeconds) return;

            this._refreshTimer = 0f;
            DynamicGI.UpdateEnvironment();
        }

        private void OnDisable()
        {
            if (this.cycle == null) return;

            this.cycle.TimeChanged -= OnTimeChanged;
        }

        private void OnDestroy()
        {
            if (this._runtimeSkybox == null) return;

            if (RenderSettings.skybox == this._runtimeSkybox) RenderSettings.skybox = this._previousSkybox;

            Destroy(this._runtimeSkybox);
        }
        #endregion

        #region Private Methods
        private void Apply(float normalizedTime)
        {
            var preset = this.cycle.Preset;
            if (preset == null) return;

            this._runtimeSkybox.SetFloat(BlendId, preset.EvaluateSkyBlend(normalizedTime));
            this._runtimeSkybox.SetFloat(ExposureId, preset.EvaluateSkyExposure(normalizedTime));
        }
        #endregion

        #region Unity Callbacks
        private void OnTimeChanged(float normalizedTime) => Apply(normalizedTime);
        #endregion
    }
}
