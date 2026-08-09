using LilyOfValley.Core;
using LilyOfValley.Inputs;
using LilyOfValley.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LilyOfValley.EditorTools.SerializedFieldUtility;

namespace LilyOfValley.EditorTools
{
    public static class TestSceneBuilder
    {
        #region Fields
        private const string TestScenePath = "Assets/Scenes/TestScene.unity";
        private const string GroundName = "Ground";
        private const string PlayerName = "Player";
        private const string PivotName = "Camera Pivot";
        private const string SystemsName = "Game Systems";

        private const float CapsuleHeight = 2f;
        private const float CapsuleRadius = 0.5f;
        #endregion

        #region Public Methods
        [MenuItem("Tools/Lily of Valley/3 - Populate Test Scene", priority = 12)]
        public static void PopulateTestScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != TestScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

                scene = EditorSceneManager.OpenScene(TestScenePath, OpenSceneMode.Single);
            }

            var reader = GameScaffolder.EnsureInputReader();
            if (reader == null)
            {
                Debug.LogError($"{nameof(TestSceneBuilder)}: could not resolve an {nameof(InputReader)}; aborting.");
                return;
            }

            CreateGround(scene);

            var player = CreatePlayer(scene, reader);
            if (player != null) AttachCamera(player.transform.Find(PivotName));

            EnsureBootstrap(reader);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{nameof(TestSceneBuilder)}: '{TestScenePath}' updated.");
        }
        #endregion

        #region Private Methods
        private static void CreateGround(Scene scene)
        {
            var existing = FindRoot(scene, GroundName);
            if (existing != null)
            {
                GroundGridFactory.ApplyGridMaterial(existing);
                Debug.Log($"{nameof(TestSceneBuilder)}: '{GroundName}' already exists; grid material refreshed.");
                return;
            }

            var ground = GroundGridFactory.CreateGround(GroundName);
            Undo.RegisterCreatedObjectUndo(ground, "Create Ground");
        }

        private static GameObject CreatePlayer(Scene scene, InputReader reader)
        {
            if (FindRoot(scene, PlayerName) != null)
            {
                Debug.Log($"{nameof(TestSceneBuilder)}: '{PlayerName}' already exists; left untouched.");
                return null;
            }

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = PlayerName;
            player.transform.position = new Vector3(0f, CapsuleHeight * 0.5f + 0.05f, 0f);
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            Undo.RegisterCreatedObjectUndo(player, "Create Player");

            var controller = player.AddComponent<CharacterController>();
            controller.height = CapsuleHeight;
            controller.radius = CapsuleRadius;
            controller.center = Vector3.zero;
            controller.slopeLimit = 50f;
            controller.stepOffset = 0.35f;

            var pivot = new GameObject(PivotName);
            pivot.transform.SetParent(player.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            var motor = player.AddComponent<PlayerMotor>();
            ApplyFields(motor, so => SetObject(so, "inputReader", reader));

            var look = player.AddComponent<PlayerLook>();
            ApplyFields(look, so =>
            {
                SetObject(so, "inputReader", reader);
                SetObject(so, "cameraPivot", pivot.transform);
            });

            return player;
        }

        private static void AttachCamera(Transform pivot)
        {
            if (pivot == null) return;

            var camera = Camera.main;
            if (camera == null)
            {
                var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
                Undo.RegisterCreatedObjectUndo(go, "Create Main Camera");
                camera = go.GetComponent<Camera>();
            }

            Undo.SetTransformParent(camera.transform, pivot, "Attach Camera To Player");
            camera.transform.localPosition = new Vector3(0f, 0.6f, -5f);
            camera.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);
        }

        private static void EnsureBootstrap(InputReader reader)
        {
            var existing = Object.FindFirstObjectByType<GameBootstrap>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Debug.Log($"{nameof(TestSceneBuilder)}: {nameof(GameBootstrap)} already present; left untouched.");
                return;
            }

            var systems = new GameObject(SystemsName);
            Undo.RegisterCreatedObjectUndo(systems, "Create Game Systems");

            var bootstrap = systems.AddComponent<GameBootstrap>();
            ApplyFields(bootstrap, so => SetObject(so, "inputReader", reader));
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == name) return roots[i];
            }

            return null;
        }
        #endregion
    }
}
