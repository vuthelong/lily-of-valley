using Unity.Cinemachine;
using UnityEngine;

namespace LilyOfValley.Cameras
{
    [RequireComponent(typeof(CinemachineVirtualCameraBase))]
    public sealed class CameraRig : MonoBehaviour
    {
        #region Field

        public const int ActivePriority = 20;
        public const int IdlePriority = 0;

        [SerializeField] private CameraViewId id = CameraViewId.ThirdPerson;

        [SerializeField] private CinemachineVirtualCameraBase virtualCamera;

        #endregion

        #region Property

        public CameraViewId Id => this.id;

        public CinemachineVirtualCameraBase VirtualCamera => this.virtualCamera;

        public bool IsActive => this.virtualCamera != null && this.virtualCamera.Priority.Value >= ActivePriority;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (this.virtualCamera == null) TryGetComponent(out this.virtualCamera);
            if (this.virtualCamera != null) return;

            Debug.LogError($"{nameof(CameraRig)}: no {nameof(CinemachineVirtualCameraBase)} on '{name}'.", this);
            enabled = false;
        }

        private void OnEnable() => CameraManager.Register(this);

        private void OnDisable() => CameraManager.Unregister(this);

        #endregion

        #region Method

        public void SetActive(bool active)
        {
            if (this.virtualCamera == null) return;

            this.virtualCamera.Priority = active ? ActivePriority : IdlePriority;
        }

        public void SetTargets(Transform follow, Transform lookAt)
        {
            if (this.virtualCamera == null) return;

            this.virtualCamera.Follow = follow;
            this.virtualCamera.LookAt = lookAt;
        }

        #endregion
    }
}
