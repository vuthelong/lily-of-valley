using LilyOfValley.Core;
using LilyOfValley.Inputs;
using UnityEngine;

namespace LilyOfValley.Player
{
    public sealed class PlayerLook : MonoBehaviour
    {
        #region Fields
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float sensitivity = 0.08f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private bool invertY;

        private float _pitch;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (this.inputReader != null && this.cameraPivot != null) return;

            Debug.LogError($"{nameof(PlayerLook)}: {nameof(this.inputReader)} and {nameof(this.cameraPivot)} must be assigned.", this);
            enabled = false;
        }

        private void LateUpdate()
        {
            if (!CursorService.IsLocked) return;

            var delta = this.inputReader.LookInput * this.sensitivity;
            if (delta.sqrMagnitude <= 0f) return;

            transform.Rotate(Vector3.up, delta.x, Space.World);

            this._pitch += this.invertY ? delta.y : -delta.y;
            this._pitch = Mathf.Clamp(this._pitch, this.minPitch, this.maxPitch);
            this.cameraPivot.localRotation = Quaternion.Euler(this._pitch, 0f, 0f);
        }
        #endregion
    }
}
