using LilyOfValley.Core;
using LilyOfValley.Inputs;
using UnityEngine;
using UnityEngine.UI;

namespace LilyOfValley.UI
{
    public sealed class RebindMenuUI : MonoBehaviour
    {
        #region Fields
        [SerializeField] private InputReader inputReader;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resetAllButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private bool openOnStart;

        private bool _isOpen;
        #endregion

        #region Properties
        public bool IsOpen => this._isOpen;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (this.inputReader != null && this.panel != null) return;

            Debug.LogError($"{nameof(RebindMenuUI)}: {nameof(this.inputReader)} and {nameof(this.panel)} must be assigned.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            this.inputReader.MenuTogglePressed += Toggle;

            if (this.resetAllButton != null) this.resetAllButton.onClick.AddListener(OnResetAllClicked);
            if (this.closeButton != null) this.closeButton.onClick.AddListener(Close);
        }

        private void Start() => SetOpen(this.openOnStart);

        private void OnDisable()
        {
            this.inputReader.MenuTogglePressed -= Toggle;

            if (this.resetAllButton != null) this.resetAllButton.onClick.RemoveListener(OnResetAllClicked);
            if (this.closeButton != null) this.closeButton.onClick.RemoveListener(Close);
        }
        #endregion

        #region Public Methods
        public void Toggle() => SetOpen(!this._isOpen);

        public void Open() => SetOpen(true);

        public void Close() => SetOpen(false);
        #endregion

        #region Private Methods
        private void SetOpen(bool open)
        {
            this._isOpen = open;
            this.panel.SetActive(open);
            CursorService.SetLocked(!open);

            if (open) this.inputReader.DisableGameplay();
            else this.inputReader.EnableGameplay();
        }
        #endregion

        #region Unity Callbacks
        private void OnResetAllClicked() => InputRebindService.ResetAllOverrides(this.inputReader.Actions);
        #endregion
    }
}
