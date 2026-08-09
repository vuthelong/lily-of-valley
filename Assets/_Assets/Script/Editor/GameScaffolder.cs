using System;
using LilyOfValley.Core;
using LilyOfValley.Inputs;
using LilyOfValley.Player;
using LilyOfValley.UI;
using LilyOfValley.World;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static LilyOfValley.EditorTools.SerializedFieldUtility;
using static LilyOfValley.EditorTools.EditorAssetUtility;

namespace LilyOfValley.EditorTools
{
    public static class GameScaffolder
    {
        #region Fields
        private const string SettingsFolder = "Assets/_Assets/Settings";
        private const string MaterialFolder = "Assets/_Assets/Material";
        private const string SceneFolder = "Assets/_Assets/Scenes";

        private const string InputActionAssetPath = "Assets/InputSystem_Actions.inputactions";
        private const string InputReaderPath = SettingsFolder + "/InputReader.asset";
        private const string PresetPath = SettingsFolder + "/DayNightPreset.asset";
        private const string SkyboxMaterialPath = MaterialFolder + "/Skybox_DayNight.mat";
        private const string ScenePath = SceneFolder + "/Gameplay.unity";

        private const string SkyboxShaderName = "Lily of Valley/Skybox Day Night";
        private const string NightTexturePath = "Assets/_Assets/UI/Sky/skybox-night.png";
        private const string DayTexturePath = "Assets/_Assets/UI/Sky/skybox-day.png";
        #endregion

        #region Public Methods
        public static InputReader EnsureInputReader()
        {
            EnsureFolder(SettingsFolder);

            var reader = LoadOrCreate<InputReader>(InputReaderPath);
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionAssetPath);
            if (inputAsset == null)
            {
                Debug.LogWarning($"{nameof(GameScaffolder)}: '{InputActionAssetPath}' not found; assign an {nameof(InputActionAsset)} to '{InputReaderPath}' manually.");
                return reader;
            }

            ApplyFields(reader, so => SetObject(so, "actions", inputAsset));
            EditorUtility.SetDirty(reader);
            AssetDatabase.SaveAssets();
            return reader;
        }

        [MenuItem("Tools/Lily of Valley/1 - Create Config Assets", priority = 10)]
        public static void CreateConfigAssets()
        {
            EnsureFolder(MaterialFolder);
            EnsureInputReader();

            var preset = LoadOrCreate<DayNightPreset>(PresetPath);
            preset.ApplyDefaults();
            EditorUtility.SetDirty(preset);

            CreateSkyboxMaterial();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{nameof(GameScaffolder)}: config assets ready under '{SettingsFolder}'.");
        }

        [MenuItem("Tools/Lily of Valley/2 - Build Gameplay Scene", priority = 11)]
        public static void BuildGameplayScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            CreateConfigAssets();

            var reader = AssetDatabase.LoadAssetAtPath<InputReader>(InputReaderPath);
            var preset = AssetDatabase.LoadAssetAtPath<DayNightPreset>(PresetPath);
            var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEnvironment();
            var sun = CreateDirectionalLight("Sun", new Color(1f, 0.96f, 0.88f), 1.2f, 50f);
            var moon = CreateDirectionalLight("Moon", new Color(0.55f, 0.65f, 0.95f), 0.2f, -130f);
            var cycle = CreateSystems(reader, preset, skybox, sun, moon);

            CreatePlayer(reader);
            CreateUserInterface(reader, cycle);

            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;

            EnsureFolder(SceneFolder);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"{nameof(GameScaffolder)}: gameplay scene saved to '{ScenePath}'.");
        }
        #endregion

        #region Private Methods
        private static DayNightCycle CreateSystems(InputReader reader, DayNightPreset preset, Material skybox, Light sun, Light moon)
        {
            var systems = new GameObject("Game Systems");

            var bootstrap = systems.AddComponent<GameBootstrap>();
            ApplyFields(bootstrap, so => SetObject(so, "inputReader", reader));

            var cycle = systems.AddComponent<DayNightCycle>();
            ApplyFields(cycle, so => SetObject(so, "preset", preset));

            var lighting = systems.AddComponent<DayNightLighting>();
            ApplyFields(lighting, so =>
            {
                SetObject(so, "cycle", cycle);
                SetObject(so, "sunLight", sun);
                SetObject(so, "moonLight", moon);
            });

            var sky = systems.AddComponent<DayNightSkybox>();
            ApplyFields(sky, so =>
            {
                SetObject(so, "cycle", cycle);
                SetObject(so, "skyboxTemplate", skybox);
            });

            return cycle;
        }

        private static void CreateEnvironment()
        {
            GroundGridFactory.CreateGround("Ground");

            var landmarks = new GameObject("Landmarks");
            for (var i = 0; i < 8; i++)
            {
                var angle = i / 8f * Mathf.PI * 2f;
                var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = $"Block {i}";
                block.transform.SetParent(landmarks.transform, false);
                block.transform.position = new Vector3(Mathf.Cos(angle) * 14f, 1.5f, Mathf.Sin(angle) * 14f);
                block.transform.localScale = new Vector3(2f, 3f, 2f);
                block.isStatic = true;
            }
        }

        private static Light CreateDirectionalLight(string name, Color color, float intensity, float pitch)
        {
            var go = new GameObject(name);
            go.transform.rotation = Quaternion.Euler(pitch, 170f, 0f);

            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            return light;
        }

        private static void CreatePlayer(InputReader reader)
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0.2f, -6f);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.32f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.slopeLimit = 50f;
            controller.stepOffset = 0.35f;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.64f, 0.9f, 0.64f);
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var pivot = new GameObject("Camera Pivot");
            pivot.transform.SetParent(player.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 1.62f, 0f);

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(pivot.transform, false);

            var motor = player.AddComponent<PlayerMotor>();
            ApplyFields(motor, so => SetObject(so, "inputReader", reader));

            var look = player.AddComponent<PlayerLook>();
            ApplyFields(look, so =>
            {
                SetObject(so, "inputReader", reader);
                SetObject(so, "cameraPivot", pivot.transform);
            });
        }

        private static void CreateUserInterface(InputReader reader, DayNightCycle cycle)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvasGo = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var clock = CreateText("Clock", canvasGo.transform, 34f, TextAlignmentOptions.TopLeft);
            AnchorCorner(clock.rectTransform, new Vector2(0f, 1f), new Vector2(32f, -24f), new Vector2(640f, 48f));

            var clockUI = canvasGo.AddComponent<ClockUI>();
            ApplyFields(clockUI, so =>
            {
                SetObject(so, "cycle", cycle);
                SetObject(so, "label", clock);
            });

            var hint = CreateText("Hint", canvasGo.transform, 22f, TextAlignmentOptions.BottomLeft);
            hint.text = "WASD move - Space jump - Shift sprint - Esc key bindings";
            AnchorCorner(hint.rectTransform, new Vector2(0f, 0f), new Vector2(32f, 24f), new Vector2(900f, 40f));

            var panel = CreateRebindPanel(canvasGo.transform, reader, out var resetAllButton, out var closeButton);

            var menu = canvasGo.AddComponent<RebindMenuUI>();
            ApplyFields(menu, so =>
            {
                SetObject(so, "inputReader", reader);
                SetObject(so, "panel", panel.gameObject);
                SetObject(so, "resetAllButton", resetAllButton);
                SetObject(so, "closeButton", closeButton);
            });
        }

        private static RectTransform CreateRebindPanel(Transform parent, InputReader reader, out Button resetAllButton, out Button closeButton)
        {
            var panel = CreateUIObject("Rebind Panel", parent);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = new Vector2(760f, 600f);

            var background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.06f, 0.09f, 0.94f);

            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var title = CreateText("Title", panel, 32f, TextAlignmentOptions.Center);
            title.text = "Key Bindings";
            AddLayoutElement(title.gameObject, 0f, 48f);

            CreateRebindRows(panel, reader);

            resetAllButton = CreateButton("Reset All", panel, 0f, out var resetAllLabel);
            resetAllLabel.text = "Reset All";
            AddLayoutElement(resetAllButton.gameObject, 0f, 48f);

            closeButton = CreateButton("Close", panel, 0f, out var closeLabel);
            closeLabel.text = "Close";
            AddLayoutElement(closeButton.gameObject, 0f, 48f);

            return panel;
        }

        private static void CreateRebindRows(RectTransform parent, InputReader reader)
        {
            var asset = reader != null ? reader.Actions : null;
            var map = asset != null ? asset.FindActionMap(InputReader.PlayerMapName) : null;
            if (map == null)
            {
                Debug.LogWarning($"{nameof(GameScaffolder)}: action map '{InputReader.PlayerMapName}' not found; rebind rows were skipped.");
                return;
            }

            CreateCompositeRow(parent, reader, map, InputReader.MoveActionName, "up", "Move Forward");
            CreateCompositeRow(parent, reader, map, InputReader.MoveActionName, "down", "Move Back");
            CreateCompositeRow(parent, reader, map, InputReader.MoveActionName, "left", "Move Left");
            CreateCompositeRow(parent, reader, map, InputReader.MoveActionName, "right", "Move Right");
            CreateSimpleRow(parent, reader, map, InputReader.JumpActionName, "Jump");
            CreateSimpleRow(parent, reader, map, InputReader.SprintActionName, "Sprint");
            CreateSimpleRow(parent, reader, map, InputReader.InteractActionName, "Interact");
        }

        private static void CreateCompositeRow(RectTransform parent, InputReader reader, InputActionMap map, string actionName, string partName, string title)
        {
            var action = map.FindAction(actionName);
            if (action == null) return;

            CreateRow(parent, reader, map.name, actionName, FindKeyboardPartIndex(action, partName), title);
        }

        private static void CreateSimpleRow(RectTransform parent, InputReader reader, InputActionMap map, string actionName, string title)
        {
            var action = map.FindAction(actionName);
            if (action == null) return;

            CreateRow(parent, reader, map.name, actionName, FindKeyboardBindingIndex(action), title);
        }

        private static void CreateRow(RectTransform parent, InputReader reader, string mapName, string actionName, int bindingIndex, string title)
        {
            if (bindingIndex < 0)
            {
                Debug.LogWarning($"{nameof(GameScaffolder)}: no keyboard binding found for '{mapName}/{actionName}'; row '{title}' was skipped.");
                return;
            }

            var row = CreateUIObject($"Row {title}", parent);
            AddLayoutElement(row.gameObject, 0f, 48f);

            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 10f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;

            var label = CreateText("Label", row, 24f, TextAlignmentOptions.MidlineLeft);
            label.text = title;
            AddLayoutElement(label.gameObject, 260f, 44f);

            var rebindButton = CreateButton("Rebind", row, 300f, out var bindingLabel);
            var resetButton = CreateButton("Reset", row, 110f, out var resetLabel);
            resetLabel.text = "Reset";

            var component = row.gameObject.AddComponent<RebindButtonUI>();
            ApplyFields(component, so =>
            {
                SetObject(so, "inputReader", reader);
                SetString(so, "actionMapName", mapName);
                SetString(so, "actionName", actionName);
                SetInt(so, "bindingIndex", bindingIndex);
                SetString(so, "displayTitle", title);
                SetObject(so, "titleLabel", label);
                SetObject(so, "bindingLabel", bindingLabel);
                SetObject(so, "rebindButton", rebindButton);
                SetObject(so, "resetButton", resetButton);
            });
        }

        private static int FindKeyboardPartIndex(InputAction action, string partName)
        {
            for (var i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isPartOfComposite) continue;
                if (!string.Equals(binding.name, partName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!IsKeyboardBinding(binding)) continue;

                return i;
            }

            return -1;
        }

        private static int FindKeyboardBindingIndex(InputAction action)
        {
            for (var i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isComposite || binding.isPartOfComposite) continue;
                if (!IsKeyboardBinding(binding)) continue;

                return i;
            }

            return -1;
        }

        private static bool IsKeyboardBinding(InputBinding binding)
        {
            var path = binding.effectivePath;
            return !string.IsNullOrEmpty(path) && path.IndexOf("Keyboard", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Material CreateSkyboxMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
            if (material == null)
            {
                var shader = Shader.Find(SkyboxShaderName);
                if (shader == null)
                {
                    Debug.LogError($"{nameof(GameScaffolder)}: shader '{SkyboxShaderName}' not found; let Unity compile it first, then run this menu again.");
                    return null;
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, SkyboxMaterialPath);
            }

            var night = AssetDatabase.LoadAssetAtPath<Texture2D>(NightTexturePath);
            var day = AssetDatabase.LoadAssetAtPath<Texture2D>(DayTexturePath);
            if (night != null) material.SetTexture("_MainTex", night);
            if (day != null) material.SetTexture("_SecondTex", day);

            EditorUtility.SetDirty(material);
            return material;
        }

        private static RectTransform CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static TMP_Text CreateText(string name, Transform parent, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = name;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, float preferredWidth, out TMP_Text label)
        {
            var root = CreateUIObject(name, parent);

            var image = root.gameObject.AddComponent<Image>();
            image.color = new Color(0.20f, 0.23f, 0.30f, 1f);

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            if (preferredWidth > 0f) AddLayoutElement(root.gameObject, preferredWidth, 44f);

            label = CreateText("Text", root, 22f, TextAlignmentOptions.Center);
            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return button;
        }

        private static void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
        {
            var element = target.AddComponent<LayoutElement>();
            if (preferredWidth > 0f)
            {
                element.preferredWidth = preferredWidth;
                element.minWidth = preferredWidth;
                element.flexibleWidth = 0f;
            }

            if (preferredHeight <= 0f) return;

            element.preferredHeight = preferredHeight;
            element.minHeight = preferredHeight;
        }

        private static void AnchorCorner(RectTransform rect, Vector2 anchor, Vector2 offset, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
        }

        #endregion
    }
}
