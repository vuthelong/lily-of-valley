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
        #region Field

        private const string SystemName = "Camera System";
        private const string MainCameraName = "Main Camera";
        private const string MainCameraTag = "MainCamera";
        private const string ThirdPersonRigName = "CM Third Person";
        private const string FirstPersonRigName = "CM First Person";

        private const string CreateSystemUndoName = "Create Camera System";
        private const string CreateMainCameraUndoName = "Create Main Camera";
        private const string DetachMainCameraUndoName = "Detach Main Camera";

        private const string ViewIdFieldName = "id";
        private const string VirtualCameraFieldName = "virtualCamera";
        private const string BrainFieldName = "brain";
        private const string ImpulseSourceFieldName = "impulseSource";
        private const string FollowTargetFieldName = "followTarget";
        private const string LookAtTargetFieldName = "lookAtTarget";

        private static readonly Vector3 ThirdPersonOffset = new(0f, 0.6f, -5f);
        private static readonly Vector3 ThirdPersonDamping = Vector3.one * 0.15f;

        #endregion

        #region Camera System

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
            Undo.RegisterCreatedObjectUndo(system, CreateSystemUndoName);

            var impulseSource = system.AddComponent<CinemachineImpulseSource>();
            manager = system.AddComponent<CameraManager>();

            CreateRig<CinemachineHardLookAt>(system.transform, ThirdPersonRigName, CameraViewId.ThirdPerson, ThirdPersonOffset, ThirdPersonDamping);
            CreateRig<CinemachineRotateWithFollowTarget>(system.transform, FirstPersonRigName, CameraViewId.FirstPerson, Vector3.zero, Vector3.zero);

            Wire(manager, brain, impulseSource, followTarget);

            return manager;
        }

        private static Camera EnsureBrainCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var go = new GameObject(MainCameraName, typeof(Camera), typeof(AudioListener)) { tag = MainCameraTag };
                Undo.RegisterCreatedObjectUndo(go, CreateMainCameraUndoName);
                camera = go.GetComponent<Camera>();
            }

            if (camera.transform.parent != null) Undo.SetTransformParent(camera.transform, null, DetachMainCameraUndoName);

            if (!camera.TryGetComponent<CinemachineBrain>(out _)) Undo.AddComponent<CinemachineBrain>(camera.gameObject);

            return camera;
        }

        #endregion

        #region Rig Assembly

        private static void CreateRig<TAim>(Transform parent, string name, CameraViewId id, Vector3 offset, Vector3 damping) where TAim : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var virtualCamera = go.AddComponent<CinemachineCamera>();
            virtualCamera.Priority = CameraRig.IdlePriority;

            var follow = go.AddComponent<CinemachineFollow>();
            follow.FollowOffset = offset;
            follow.TrackerSettings.BindingMode = BindingMode.LockToTarget;
            follow.TrackerSettings.PositionDamping = damping;

            go.AddComponent<TAim>();
            go.AddComponent<CinemachineImpulseListener>();

            var rig = go.AddComponent<CameraRig>();
            ApplyFields(rig, so =>
            {
                SetInt(so, ViewIdFieldName, (int)id);
                SetObject(so, VirtualCameraFieldName, virtualCamera);
            });
        }

        private static void Wire(CameraManager manager, CinemachineBrain brain, CinemachineImpulseSource impulseSource, Transform followTarget)
        {
            ApplyFields(manager, so =>
            {
                if (brain != null) SetObject(so, BrainFieldName, brain);

                if (impulseSource != null) SetObject(so, ImpulseSourceFieldName, impulseSource);

                if (followTarget == null) return;

                SetObject(so, FollowTargetFieldName, followTarget);
                SetObject(so, LookAtTargetFieldName, followTarget);
            });
        }

        #endregion
    }
}
