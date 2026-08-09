using LilyOfValley.Core;
using LilyOfValley.Inputs;
using LilyOfValley.Player;
using LilyOfValley.UI;
using LilyOfValley.Units;
using LilyOfValley.Units.Behaviours;
using LilyOfValley.Units.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LilyOfValley.EditorTools.SerializedFieldUtility;

namespace LilyOfValley.EditorTools
{
    public static class TestSceneBuilder
    {
        #region Field

        private const string MenuPath = "Tools/Lily of Valley/3 - Populate Test Scene";
        private const int MenuPriority = 12;

        private const string TestScenePath = "Assets/Scenes/TestScene.unity";
        private const string GroundName = "Ground";
        private const string PlayerName = "Player";
        private const string PivotName = "Camera Pivot";
        private const string SystemsName = "Game Systems";
        private const string HudName = "HUD";

        private const string CreateGroundUndoName = "Create Ground";
        private const string CreatePlayerUndoName = "Create Player";
        private const string CreateSystemsUndoName = "Create Game Systems";
        private const string CreateHudUndoName = "Create HUD";

        private const string InputReaderFieldName = "inputReader";
        private const string CameraPivotFieldName = "cameraPivot";
        private const string CharacterDataFieldName = "data";
        private const string StartLevelFieldName = "startLevel";

        private const int PlayerStartLevel = 1;

        private const float CapsuleHeight = 2f;
        private const float CapsuleRadius = 0.5f;
        private const float CapsuleHalfHeight = CapsuleHeight * 0.5f;
        private const float GroundClearance = 0.05f;
        private const float PivotHeight = 0.6f;
        private const float SlopeLimit = 50f;
        private const float StepOffset = 0.35f;

        #endregion

        #region Scene Content

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
            Undo.RegisterCreatedObjectUndo(ground, CreateGroundUndoName);
        }

        private static GameObject EnsurePlayer(Scene scene, InputReader reader)
        {
            var existing = FindRoot(scene, PlayerName);
            if (existing != null)
            {
                Debug.Log($"{nameof(TestSceneBuilder)}: '{PlayerName}' already exists; left untouched.");
                return existing;
            }

            var player = CreatePlayerBody();

            var pivot = new GameObject(PivotName);
            pivot.transform.SetParent(player.transform, false);
            pivot.transform.localPosition = new Vector3(0f, PivotHeight, 0f);

            var motor = player.AddComponent<PlayerMotor>();
            ApplyFields(motor, serialized => SetObject(serialized, InputReaderFieldName, reader));

            var look = player.AddComponent<PlayerLook>();
            ApplyFields(look, serialized =>
            {
                SetObject(serialized, InputReaderFieldName, reader);
                SetObject(serialized, CameraPivotFieldName, pivot.transform);
            });

            return player;
        }

        private static GameObject CreatePlayerBody()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = PlayerName;
            player.transform.position = new Vector3(0f, CapsuleHalfHeight + GroundClearance, 0f);
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            Undo.RegisterCreatedObjectUndo(player, CreatePlayerUndoName);

            var controller = player.AddComponent<CharacterController>();
            controller.height = CapsuleHeight;
            controller.radius = CapsuleRadius;
            controller.center = Vector3.zero;
            controller.slopeLimit = SlopeLimit;
            controller.stepOffset = StepOffset;

            return player;
        }

        private static void EnsureCharacter(GameObject player)
        {
            if (player == null) return;

            var data = CharacterDataFactory.EnsurePlayerData();
            if (data == null)
            {
                Debug.LogWarning($"{nameof(TestSceneBuilder)}: no {nameof(CharacterData)}; {nameof(Character)} skipped.");
                return;
            }

            if (!player.TryGetComponent<Character>(out var character)) character = Undo.AddComponent<Character>(player);

            ApplyFields(character, serialized =>
            {
                SetObject(serialized, CharacterDataFieldName, data);
                SetInt(serialized, StartLevelFieldName, PlayerStartLevel);
            });

            if (player.GetComponent<CharacterRegenBehaviour>() == null) Undo.AddComponent<CharacterRegenBehaviour>(player);
        }

        private static void EnsureCameraSystem(GameObject player)
        {
            var pivot = player != null ? player.transform.Find(PivotName) : null;
            if (pivot == null)
            {
                Debug.LogWarning($"{nameof(TestSceneBuilder)}: '{PivotName}' not found under '{PlayerName}'; camera system skipped.");
                return;
            }

            CameraRigFactory.EnsureCameraSystem(pivot);
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
            Undo.RegisterCreatedObjectUndo(systems, CreateSystemsUndoName);

            var bootstrap = systems.AddComponent<GameBootstrap>();
            ApplyFields(bootstrap, serialized => SetObject(serialized, InputReaderFieldName, reader));
        }

        private static void EnsureHud(Scene scene)
        {
            if (Object.FindFirstObjectByType<FpsCounterUI>(FindObjectsInactive.Include) != null)
            {
                Debug.Log($"{nameof(TestSceneBuilder)}: {nameof(FpsCounterUI)} already present; left untouched.");
                return;
            }

            var hud = FindRoot(scene, HudName);
            if (hud == null)
            {
                hud = UiFactory.CreateCanvas(HudName).gameObject;
                Undo.RegisterCreatedObjectUndo(hud, CreateHudUndoName);
            }

            UiFactory.CreateFpsCounter(hud.transform);
        }

        #endregion

        #region Method

        [MenuItem(MenuPath, priority = MenuPriority)]
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

            var player = EnsurePlayer(scene, reader);
            EnsureCharacter(player);
            EnsureCameraSystem(player);
            EnsureBootstrap(reader);
            EnsureHud(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{nameof(TestSceneBuilder)}: '{TestScenePath}' updated.");
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
