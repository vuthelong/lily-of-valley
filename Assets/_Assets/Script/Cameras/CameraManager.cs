using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace LilyOfValley.Cameras
{
    [DefaultExecutionOrder(ExecutionOrder)]
    public sealed class CameraManager : MonoBehaviour
    {
        #region Field

        private const int ExecutionOrder = -90;
        private const bool ShouldIncludeInactiveRigs = true;

        [SerializeField] private CinemachineBrain brain;

        [SerializeField] private CinemachineImpulseSource impulseSource;

        [SerializeField] private CameraViewId defaultView = CameraViewId.ThirdPerson;

        [SerializeField] private Transform followTarget;

        [SerializeField] private Transform lookAtTarget;

        private readonly Dictionary<CameraViewId, CameraRig> _rigs = new();

        private CameraViewId _activeView = CameraViewId.None;

        public event Action<CameraViewId> ViewChanged;

        #endregion

        #region Property

        public static CameraManager Instance { get; private set; }

        public CinemachineBrain Brain => this.brain;

        public Camera OutputCamera => this.brain == null ? null : this.brain.OutputCamera;

        public CameraViewId ActiveView => this._activeView;

        public bool IsBlending => this.brain != null && this.brain.IsBlending;

        public Transform FollowTarget => this.followTarget;

        public Transform LookAtTarget => this.lookAtTarget;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"{nameof(CameraManager)}: an instance already exists; destroying the one on '{name}'.", this);
                Destroy(this);
                return;
            }

            Instance = this;
            ResolveBrain();
            CollectRigs();
        }

        private void Start()
        {
            ApplyTargets();
            SwitchTo(this.defaultView);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region Rig Registry

        public static void Register(CameraRig rig)
        {
            if (Instance == null) return;

            Instance.Add(rig);
        }

        public static void Unregister(CameraRig rig)
        {
            if (Instance == null) return;

            Instance.Remove(rig);
        }

        public bool TryGetRig(CameraViewId view, out CameraRig rig) => this._rigs.TryGetValue(view, out rig);

        private void Add(CameraRig rig)
        {
            if (rig == null || rig.Id == CameraViewId.None) return;

            this._rigs[rig.Id] = rig;
            rig.SetActive(rig.Id == this._activeView);

            if (this.followTarget == null) return;

            rig.SetTargets(this.followTarget, ResolveLookAt());
        }

        private void Remove(CameraRig rig)
        {
            if (rig == null) return;

            if (!this._rigs.TryGetValue(rig.Id, out var registered) || registered != rig) return;

            this._rigs.Remove(rig.Id);
        }

        private void CollectRigs()
        {
            var rigs = GetComponentsInChildren<CameraRig>(ShouldIncludeInactiveRigs);

            for (var i = 0; i < rigs.Length; i++)
            {
                Add(rigs[i]);
            }
        }

        #endregion

        #region View Switching

        public void SwitchTo(CameraViewId view)
        {
            if (view == CameraViewId.None || view == this._activeView) return;

            if (!this._rigs.ContainsKey(view))
            {
                Debug.LogWarning($"{nameof(CameraManager)}: no {nameof(CameraRig)} registered for '{view}'.", this);
                return;
            }

            foreach (var pair in this._rigs)
            {
                pair.Value.SetActive(pair.Key == view);
            }

            this._activeView = view;
            this.ViewChanged?.Invoke(view);
        }

        public void CutTo(CameraViewId view)
        {
            SwitchTo(view);

            if (this.brain == null) return;

            this.brain.ResetState();
        }

        #endregion

        #region Target Assignment

        public void SetTargets(Transform follow, Transform lookAt)
        {
            this.followTarget = follow;
            this.lookAtTarget = lookAt;
            ApplyTargets();
        }

        public void SetFollowTarget(Transform follow) => SetTargets(follow, this.lookAtTarget);

        public void SetLookAtTarget(Transform lookAt) => SetTargets(this.followTarget, lookAt);

        private void ApplyTargets()
        {
            if (this.followTarget == null) return;

            var lookAt = ResolveLookAt();

            foreach (var pair in this._rigs)
            {
                pair.Value.SetTargets(this.followTarget, lookAt);
            }
        }

        private Transform ResolveLookAt() => this.lookAtTarget != null ? this.lookAtTarget : this.followTarget;

        #endregion

        #region Impulse Shake

        public void Shake(float force)
        {
            if (!HasImpulseSource()) return;

            this.impulseSource.GenerateImpulseWithForce(force);
        }

        public void Shake(Vector3 velocity)
        {
            if (!HasImpulseSource()) return;

            this.impulseSource.GenerateImpulseWithVelocity(velocity);
        }

        private bool HasImpulseSource()
        {
            if (this.impulseSource != null) return true;

            Debug.LogWarning($"{nameof(CameraManager)}: no {nameof(CinemachineImpulseSource)} assigned; shake ignored.", this);
            return false;
        }

        #endregion

        #region Lens Control

        public void SetFieldOfView(CameraViewId view, float fieldOfView)
        {
            if (!TryGetRig(view, out var rig)) return;

            if (rig.VirtualCamera is not CinemachineCamera camera) return;

            camera.Lens.FieldOfView = fieldOfView;
        }

        public void SetFieldOfView(float fieldOfView) => SetFieldOfView(this._activeView, fieldOfView);

        #endregion

        #region Method

        private void ResolveBrain()
        {
            if (this.brain != null) return;

            var main = Camera.main;
            if (main != null && main.TryGetComponent(out this.brain)) return;

            Debug.LogError($"{nameof(CameraManager)}: no {nameof(CinemachineBrain)} assigned and none found on the main camera.", this);
        }

        #endregion
    }
}
