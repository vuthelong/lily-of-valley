using LilyOfValley.Cameras;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static LilyOfValley.EditorTools.SerializedFieldUtility;

namespace LilyOfValley.EditorTools
{
    public static class CameraRigFactory
    {
        #region Fields
        private const string SystemName = "Camera System";
        private const string MainCameraName = "Main Camera";
        private const string ThirdPersonRigName = "CM Third Person";
        private const string FirstPersonRigName = "CM First Person";

        private static readonly Vector3 ThirdPersonOffset = new(0f, 0.6f, -5f);
        private static readonly Vector3 ThirdPersonDamping = Vector3.one * 0.15f;
        #endregion

        #region Public Methods
        public static CameraManager EnsureCameraSystem(Transform followTarget)
        {
            var camera = EnsureBrainCamera();
            if (camera == null) return null;

            camera.TryGetComponent<CinemachineBrain>(out var brain);

            var manager = Object.FindFirstObjectByType<CameraManager>(FindObjectsInactive.Include);
            if (manager != null)
            {
                Wire(manager, brain, null, followTarget);
                Debug.Log($"{nameof(CameraRigFactory)}: {nameof(CameraManager)} already present; targets refreshed.");
                return manager;
            }

            var system = new GameObject(SystemName);
            Undo.RegisterCreatedObjectUndo(system, "Create Camera System");

            var impulseSource = system.AddComponent<CinemachineImpulseSource>();
            manager = system.AddComponent<CameraManager>();

            CreateRig(system.transform, ThirdPersonRigName, CameraViewId.ThirdPerson, ThirdPersonOffset, ThirdPersonDamping, true);
            CreateRig(system.transform, FirstPersonRigName, CameraViewId.FirstPerson, Vector3.zero, Vector3.zero, false);

            Wire(manager, brain, impulseSource, followTarget);
            return manager;
        }
        #endregion

        #region Private Methods
        private static Camera EnsureBrainCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var go = new GameObject(MainCameraName, typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
                Undo.RegisterCreatedObjectUndo(go, "Create Main Camera");
                camera = go.GetComponent<Camera>();
            }

            // The brain drives the camera transform, so it must not be parented to the player rig.
            if (camera.transform.parent != null) Undo.SetTransformParent(camera.transform, null, "Detach Main Camera");

            if (!camera.TryGetComponent<CinemachineBrain>(out _)) Undo.AddComponent<CinemachineBrain>(camera.gameObject);

            return camera;
        }

        private static void CreateRig(Transform parent, string name, CameraViewId id, Vector3 offset, Vector3 damping, bool lookAtTarget)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var virtualCamera = go.AddComponent<CinemachineCamera>();
            virtualCamera.Priority = CameraRig.IdlePriority;

            var follow = go.AddComponent<CinemachineFollow>();
            follow.FollowOffset = offset;
            follow.TrackerSettings.BindingMode = BindingMode.LockToTarget;
            follow.TrackerSettings.PositionDamping = damping;

            if (lookAtTarget) go.AddComponent<CinemachineHardLookAt>();
            else go.AddComponent<CinemachineRotateWithFollowTarget>();

            go.AddComponent<CinemachineImpulseListener>();

            var rig = go.AddComponent<CameraRig>();
            ApplyFields(rig, so =>
            {
                SetInt(so, "id", (int)id);
                SetObject(so, "virtualCamera", virtualCamera);
            });
        }

        private static void Wire(CameraManager manager, CinemachineBrain brain, CinemachineImpulseSource impulseSource, Transform followTarget)
        {
            ApplyFields(manager, so =>
            {
                if (brain != null) SetObject(so, "brain", brain);
                if (impulseSource != null) SetObject(so, "impulseSource", impulseSource);
                if (followTarget == null) return;

                SetObject(so, "followTarget", followTarget);
                SetObject(so, "lookAtTarget", followTarget);
            });
        }
        #endregion
    }
}
