using System;
using LilyOfValley.Inputs;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LilyOfValley.UI
{
    public sealed class RebindButtonUI : MonoBehaviour
    {
        #region Field

        [SerializeField] private InputReader inputReader;

        [SerializeField] private string actionMapName = InputReader.PlayerMapName;

        [SerializeField] private string actionName = InputReader.MoveActionName;

        [SerializeField] private int bindingIndex;

        [SerializeField] private string displayTitle;

        [SerializeField] private TMP_Text titleLabel;

        [SerializeField] private TMP_Text bindingLabel;

        [SerializeField] private Button rebindButton;

        [SerializeField] private Button resetButton;

        [SerializeField] private string waitingText = "< press a key >";

        private InputAction _action;
        private InputActionRebindingExtensions.RebindingOperation _operation;

        public event Action<RebindButtonUI> RebindStarted;

        public event Action<RebindButtonUI> RebindFinished;

        #endregion

        #region Property

        public bool IsRebinding => this._operation != null;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (!Resolve())
            {
                enabled = false;
                return;
            }

            if (this.titleLabel != null) this.titleLabel.text = BuildTitle();
        }

        private void OnEnable()
        {
            if (this.rebindButton != null) this.rebindButton.onClick.AddListener(OnRebindClicked);
            if (this.resetButton != null) this.resetButton.onClick.AddListener(OnResetClicked);

            InputRebindService.BindingChanged += OnBindingChanged;
            Refresh();
        }

        private void OnDisable()
        {
            CancelRebind();

            if (this.rebindButton != null) this.rebindButton.onClick.RemoveListener(OnRebindClicked);
            if (this.resetButton != null) this.resetButton.onClick.RemoveListener(OnResetClicked);

            InputRebindService.BindingChanged -= OnBindingChanged;
        }

        #endregion

        #region Action Setup

        private bool Resolve()
        {
            if (this.inputReader == null)
            {
                Debug.LogError($"{nameof(RebindButtonUI)}: {nameof(this.inputReader)} is not assigned.", this);
                return false;
            }

            this.inputReader.Initialize();

            var asset = this.inputReader.Actions;
            var map = asset != null ? asset.FindActionMap(this.actionMapName) : null;
            this._action = map != null ? map.FindAction(this.actionName) : null;

            if (this._action != null) return true;

            Debug.LogError($"{nameof(RebindButtonUI)}: action '{this.actionMapName}/{this.actionName}' was not found.", this);
            return false;
        }

        private string BuildTitle()
        {
            if (!string.IsNullOrEmpty(this.displayTitle)) return this.displayTitle;

            if (this.bindingIndex < 0 || this.bindingIndex >= this._action.bindings.Count) return this._action.name;

            var binding = this._action.bindings[this.bindingIndex];

            return string.IsNullOrEmpty(binding.name) ? this._action.name : $"{this._action.name} {binding.name}";
        }

        #endregion

        #region Display

        public void Refresh()
        {
            if (this.bindingLabel == null || this._action == null) return;

            this.bindingLabel.text = InputRebindService.GetDisplayString(this._action, this.bindingIndex);
        }

        private void SetInteractable(bool interactable)
        {
            if (this.rebindButton != null) this.rebindButton.interactable = interactable;
            if (this.resetButton != null) this.resetButton.interactable = interactable;
        }

        private void OnBindingChanged(InputAction action)
        {
            if (action != null && action != this._action) return;

            Refresh();
        }

        #endregion

        #region Rebind Flow

        public void CancelRebind()
        {
            if (this._operation == null) return;

            this._operation.Cancel();
            this._operation = null;
        }

        private void OnRebindClicked()
        {
            if (this._action == null || this._operation != null) return;

            SetInteractable(false);
            if (this.bindingLabel != null) this.bindingLabel.text = this.waitingText;
            RebindStarted?.Invoke(this);

            this._operation = InputRebindService.StartRebind(this._action, this.bindingIndex, OnRebindFinished);
            if (this._operation == null) OnRebindFinished(false);
        }

        private void OnRebindFinished(bool succeeded)
        {
            this._operation = null;
            SetInteractable(true);
            Refresh();
            RebindFinished?.Invoke(this);
        }

        private void OnResetClicked()
        {
            if (this._action == null || this._operation != null) return;

            InputRebindService.ResetBinding(this._action, this.bindingIndex);
        }

        #endregion
    }
}
