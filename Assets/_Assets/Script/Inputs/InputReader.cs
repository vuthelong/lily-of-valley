using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LilyOfValley.Inputs
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Lily of Valley/Input/Input Reader")]
    public sealed class InputReader : ScriptableObject
    {
        #region Field

        public const string PlayerMapName = "Player";
        public const string UIMapName = "UI";
        public const string MoveActionName = "Move";
        public const string LookActionName = "Look";
        public const string JumpActionName = "Jump";
        public const string SprintActionName = "Sprint";
        public const string InteractActionName = "Interact";
        public const string CancelActionName = "Cancel";

        [SerializeField] private InputActionAsset actions;

        private InputActionMap _playerMap;
        private InputActionMap _uiMap;
        private InputAction _move;
        private InputAction _look;
        private InputAction _jump;
        private InputAction _sprint;
        private InputAction _interact;
        private InputAction _cancel;
        private bool _isInitialized;

        public event Action JumpPressed;
        public event Action InteractPressed;
        public event Action MenuTogglePressed;

        #endregion

        #region Property

        public InputActionAsset Actions => this.actions;

        public Vector2 MoveInput { get; private set; }

        public Vector2 LookInput { get; private set; }

        public bool IsSprinting { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void OnDisable() => Shutdown();

        #endregion

        #region Initialization

        public void Initialize()
        {
            if (this._isInitialized) return;

            if (this.actions == null)
            {
                Debug.LogError($"{nameof(InputReader)}: no {nameof(InputActionAsset)} assigned.", this);
                return;
            }

            this._playerMap = this.actions.FindActionMap(PlayerMapName, true);
            this._uiMap = this.actions.FindActionMap(UIMapName, true);

            this._move = this._playerMap.FindAction(MoveActionName, true);
            this._look = this._playerMap.FindAction(LookActionName, true);
            this._jump = this._playerMap.FindAction(JumpActionName, true);
            this._sprint = this._playerMap.FindAction(SprintActionName, true);
            this._interact = this._playerMap.FindAction(InteractActionName, true);
            this._cancel = this._uiMap.FindAction(CancelActionName, true);

            InputRebindService.LoadOverrides(this.actions);
            Subscribe();
            this._isInitialized = true;
        }

        private void Shutdown()
        {
            Unsubscribe();

            if (this.actions != null) this.actions.Disable();

            ClearValues();
            this._isInitialized = false;
        }

        #endregion

        #region Map Activation

        public void EnableGameplay()
        {
            Initialize();
            if (this._playerMap == null) return;

            this._playerMap.Enable();
        }

        public void DisableGameplay()
        {
            if (this._playerMap == null) return;

            this._playerMap.Disable();
            ClearValues();
        }

        public void EnableUI()
        {
            Initialize();
            if (this._uiMap == null) return;

            this._uiMap.Enable();
        }

        public void DisableUI()
        {
            if (this._uiMap == null) return;

            this._uiMap.Disable();
        }

        public void DisableAll()
        {
            if (this.actions == null) return;

            this.actions.Disable();
            ClearValues();
        }

        #endregion

        #region Action Subscription

        private void Subscribe()
        {
            this._move.performed += OnMove;
            this._move.canceled += OnMove;
            this._look.performed += OnLook;
            this._look.canceled += OnLook;
            this._sprint.performed += OnSprint;
            this._sprint.canceled += OnSprint;
            this._jump.performed += OnJump;
            this._interact.performed += OnInteract;
            this._cancel.performed += OnCancel;
        }

        private void Unsubscribe()
        {
            if (!this._isInitialized) return;

            this._move.performed -= OnMove;
            this._move.canceled -= OnMove;
            this._look.performed -= OnLook;
            this._look.canceled -= OnLook;
            this._sprint.performed -= OnSprint;
            this._sprint.canceled -= OnSprint;
            this._jump.performed -= OnJump;
            this._interact.performed -= OnInteract;
            this._cancel.performed -= OnCancel;
        }

        #endregion

        #region Input Callback

        private void OnMove(InputAction.CallbackContext context) => MoveInput = context.ReadValue<Vector2>();

        private void OnLook(InputAction.CallbackContext context) => LookInput = context.ReadValue<Vector2>();

        private void OnSprint(InputAction.CallbackContext context) => IsSprinting = context.ReadValueAsButton();

        private void OnJump(InputAction.CallbackContext context) => this.JumpPressed?.Invoke();

        private void OnInteract(InputAction.CallbackContext context) => this.InteractPressed?.Invoke();

        private void OnCancel(InputAction.CallbackContext context) => this.MenuTogglePressed?.Invoke();

        #endregion

        #region Method

        private void ClearValues()
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            IsSprinting = false;
        }

        #endregion
    }
}
