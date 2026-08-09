using TMPro;
using UnityEngine;

namespace LilyOfValley.UI
{
    public sealed class FpsCounterUI : MonoBehaviour
    {
        #region Field

        private const string FullFormat = "{0:0} FPS  {1:1} ms";
        private const string CompactFormat = "{0:0} FPS";
        private const float MinSampleInterval = 0.05f;
        private const float MillisecondsPerSecond = 1000f;

        [SerializeField] private TMP_Text label;

        [SerializeField, Min(MinSampleInterval)] private float sampleInterval = 0.5f;

        [SerializeField] private bool showFrameTime = true;

        [SerializeField] private bool colorize = true;

        [SerializeField] private float warningFps = 45f;

        [SerializeField] private float criticalFps = 25f;

        [SerializeField] private Color goodColor = new(0.55f, 0.95f, 0.60f);

        [SerializeField] private Color warningColor = new(0.98f, 0.85f, 0.40f);

        [SerializeField] private Color criticalColor = new(0.98f, 0.45f, 0.40f);

        private float _elapsed;
        private int _frames;

        #endregion

        #region Property

        public float CurrentFps { get; private set; }

        public float FrameTimeMs => CurrentFps > 0f ? MillisecondsPerSecond / CurrentFps : 0f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (this.label != null) return;

            Debug.LogError($"{nameof(FpsCounterUI)}: {nameof(this.label)} is not assigned.", this);
            enabled = false;
        }

        private void OnEnable() => ResetSample();

        private void Update()
        {
            this._elapsed += Time.unscaledDeltaTime;
            this._frames++;

            if (this._elapsed < this.sampleInterval) return;

            CurrentFps = this._frames / this._elapsed;
            ResetSample();
            Render();
        }

        #endregion

        #region Rendering

        private void Render()
        {
            if (this.showFrameTime) this.label.SetText(FullFormat, CurrentFps, FrameTimeMs);
            else this.label.SetText(CompactFormat, CurrentFps);

            if (!this.colorize) return;

            this.label.color = ResolveColor();
        }

        private Color ResolveColor()
        {
            if (CurrentFps < this.criticalFps) return this.criticalColor;

            if (CurrentFps < this.warningFps) return this.warningColor;

            return this.goodColor;
        }

        #endregion

        #region Method

        public void SetVisible(bool visible) => this.label.enabled = visible;

        private void ResetSample()
        {
            this._elapsed = 0f;
            this._frames = 0;
        }

        #endregion
    }
}
