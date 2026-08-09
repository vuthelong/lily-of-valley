using LilyOfValley.World;
using TMPro;
using UnityEngine;

namespace LilyOfValley.UI
{
    public sealed class ClockUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private DayNightCycle cycle;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string format = "Day {0}  {1:00}:{2:00}  ({3})";

        private int _lastMinute = -1;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (this.cycle != null && this.label != null) return;

            Debug.LogError($"{nameof(ClockUI)}: {nameof(this.cycle)} and {nameof(this.label)} must be assigned.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            this.cycle.TimeChanged += OnTimeChanged;
            this._lastMinute = -1;
            Refresh();
        }

        private void OnDisable() => this.cycle.TimeChanged -= OnTimeChanged;
        #endregion

        #region Private Methods
        private void Refresh()
        {
            var minute = this.cycle.Minute;
            if (minute == this._lastMinute) return;

            this._lastMinute = minute;
            this.label.text = string.Format(this.format, this.cycle.Day, Mathf.FloorToInt(this.cycle.Hour), minute, this.cycle.Phase);
        }
        #endregion

        #region Unity Callbacks
        private void OnTimeChanged(float normalizedTime) => Refresh();
        #endregion
    }
}
