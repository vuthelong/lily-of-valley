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
using static LilyOfValley.EditorTools.UiFactory;

namespace LilyOfValley.EditorTools
{
    public static class GameScaffolder
    {
        #region Field

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

        private const string ConfigAssetsMenuPath = "Tools/Lily of Valley/1 - Create Config Assets";
        private const string GameplaySceneMenuPath = "Tools/Lily of Valley/2 - Build Gameplay Scene";
        private const int ConfigAssetsMenuPriority = 10;
        private const int GameplaySceneMenuPriority = 11;

        private const string SystemsName = "Game Systems";
        private const string LandmarksName = "Landmarks";
        private const string GroundName = "Ground";
        private const string BlockNamePrefix = "Block ";
        private const string SunName = "Sun";
        private const string MoonName = "Moon";
        private const string PlayerName = "Player";
        private const string PlayerBodyName = "Body";
        private const string CameraPivotName = "Camera Pivot";
        private const string EventSystemName = "EventSystem";
        private const string CanvasName = "UI";
        private const string ClockName = "Clock";
        private const string HintName = "Hint";
        private const string RebindPanelName = "Rebind Panel";
        private const string PanelTitleName = "Title";
        private const string RowNamePrefix = "Row ";
        private const string RowLabelName = "Label";
        private const string RebindButtonName = "Rebind";

        private const string HintText = "WASD move - Space jump - Shift sprint - Esc key bindings";
        private const string PanelTitleText = "Key Bindings";
        private const string ResetAllText = "Reset All";
        private const string CloseText = "Close";
        private const string ResetText = "Reset";

        private const string MoveForwardTitle = "Move Forward";
        private const string MoveBackTitle = "Move Back";
        private const string MoveLeftTitle = "Move Left";
        private const string MoveRightTitle = "Move Right";
        private const string JumpTitle = "Jump";
        private const string SprintTitle = "Sprint";
        private const string InteractTitle = "Interact";

        private const string MoveUpPartName = "up";
        private const string MoveDownPartName = "down";
        private const string MoveLeftPartName = "left";
        private const string MoveRightPartName = "right";
        private const string KeyboardDeviceToken = "Keyboard";
        private const int InvalidBindingIndex = -1;

        private const string ActionsFieldName = "actions";
        private const string InputReaderFieldName = "inputReader";
        private const string PresetFieldName = "preset";
        private const string CycleFieldName = "cycle";
        private const string SunLightFieldName = "sunLight";
        private const string MoonLightFieldName = "moonLight";
        private const string SkyboxTemplateFieldName = "skyboxTemplate";
        private const string CameraPivotFieldName = "cameraPivot";
        private const string LabelFieldName = "label";
        private const string PanelFieldName = "panel";
        private const string ResetAllButtonFieldName = "resetAllButton";
        private const string CloseButtonFieldName = "closeButton";
        private const string ActionMapNameFieldName = "actionMapName";
        private const string ActionNameFieldName = "actionName";
        private const string BindingIndexFieldName = "bindingIndex";
        private const string DisplayTitleFieldName = "displayTitle";
        private const string TitleLabelFieldName = "titleLabel";
        private const string BindingLabelFieldName = "bindingLabel";
        private const string RebindButtonFieldName = "rebindButton";
        private const string ResetButtonFieldName = "resetButton";

        private const int LandmarkCount = 8;
        private const float LandmarkRadius = 14f;
        private const float LandmarkHeight = 1.5f;
        private const float FullTurnRadians = Mathf.PI * 2f;

        private const float LightYaw = 170f;
        private const float SunIntensity = 1.2f;
        private const float SunPitch = 50f;
        private const float MoonIntensity = 0.2f;
        private const float MoonPitch = -130f;

        private const float ControllerHeight = 1.8f;
        private const float ControllerRadius = 0.32f;
        private const float ControllerSlopeLimit = 50f;
        private const float ControllerStepOffset = 0.35f;

        private const float ClockFontSize = 34f;
        private const float HintFontSize = 22f;
        private const float PanelTitleFontSize = 32f;
        private const float RowLabelFontSize = 24f;

        private const int PanelPadding = 24;
        private const float PanelSpacing = 10f;
        private const float PanelElementHeight = 48f;
        private const float RowSpacing = 10f;
        private const float RowLabelWidth = 260f;
        private const float RowLabelHeight = 44f;
        private const float RebindButtonWidth = 300f;
        private const float ResetButtonWidth = 110f;
        private const float NoPreferredWidth = 0f;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int SecondTexId = Shader.PropertyToID("_SecondTex");

        private static readonly Color SunColor = new(1f, 0.96f, 0.88f);
        private static readonly Color MoonColor = new(0.55f, 0.65f, 0.95f);
        private static readonly Color PanelBackgroundColor = new(0.05f, 0.06f, 0.09f, 0.94f);

        private static readonly Vector3 LandmarkScale = new(2f, 3f, 2f);
        private static readonly Vector3 PlayerStartPosition = new(0f, 0.2f, -6f);
        private static readonly Vector3 ControllerCenter = new(0f, 0.9f, 0f);
        private static readonly Vector3 PlayerBodyPosition = new(0f, 0.9f, 0f);
        private static readonly Vector3 PlayerBodyScale = new(0.64f, 0.9f, 0.64f);
        private static readonly Vector3 CameraPivotPosition = new(0f, 1.62f, 0f);

        private static readonly Vector2 ClockAnchor = new(0f, 1f);
        private static readonly Vector2 ClockOffset = new(32f, -24f);
        private static readonly Vector2 ClockSize = new(640f, 48f);
        private static readonly Vector2 HintAnchor = new(0f, 0f);
        private static readonly Vector2 HintOffset = new(32f, 24f);
        private static readonly Vector2 HintSize = new(900f, 40f);
        private static readonly Vector2 PanelAnchor = new(0.5f, 0.5f);
        private static readonly Vector2 PanelSize = new(760f, 600f);

        #endregion

        #region Entry Point

        [MenuItem(ConfigAssetsMenuPath, priority = ConfigAssetsMenuPriority)]
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

        [MenuItem(GameplaySceneMenuPath, priority = GameplaySceneMenuPriority)]
        public static void BuildGameplayScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            CreateConfigAssets();

            var reader = AssetDatabase.LoadAssetAtPath<InputReader>(InputReaderPath);
            var preset = AssetDatabase.LoadAssetAtPath<DayNightPreset>(PresetPath);
            var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEnvironment();
            var sun = CreateDirectionalLight(SunName, SunColor, SunIntensity, SunPitch);
            var moon = CreateDirectionalLight(MoonName, MoonColor, MoonIntensity, MoonPitch);
            var cycle = CreateSystems(reader, preset, skybox, sun, moon);

            CameraRigFactory.EnsureCameraSystem(CreatePlayer(reader));
            CreateUserInterface(reader, cycle);

            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;

            EnsureFolder(SceneFolder);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"{nameof(GameScaffolder)}: gameplay scene saved to '{ScenePath}'.");
        }

        #endregion

        #region Config Asset

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

            ApplyFields(reader, so => SetObject(so, ActionsFieldName, inputAsset));
            EditorUtility.SetDirty(reader);
            AssetDatabase.SaveAssets();

            return reader;
        }

        private static Material CreateSkyboxMaterial()
        {
            var material = LoadOrCreateSkyboxMaterial();
            if (material == null) return null;

            var night = AssetDatabase.LoadAssetAtPath<Texture2D>(NightTexturePath);
            var day = AssetDatabase.LoadAssetAtPath<Texture2D>(DayTexturePath);
            if (night != null) material.SetTexture(MainTexId, night);

            if (day != null) material.SetTexture(SecondTexId, day);

            EditorUtility.SetDirty(material);

            return material;
        }

        private static Material LoadOrCreateSkyboxMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
            if (material != null) return material;

            var shader = Shader.Find(SkyboxShaderName);
            if (shader == null)
            {
                Debug.LogError($"{nameof(GameScaffolder)}: shader '{SkyboxShaderName}' not found; let Unity compile it first, then run this menu again.");
                return null;
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, SkyboxMaterialPath);

            return material;
        }

        #endregion

        #region Scene Content

        private static DayNightCycle CreateSystems(InputReader reader, DayNightPreset preset, Material skybox, Light sun, Light moon)
        {
            var systems = new GameObject(SystemsName);

            var bootstrap = systems.AddComponent<GameBootstrap>();
            ApplyFields(bootstrap, so => SetObject(so, InputReaderFieldName, reader));

            var cycle = systems.AddComponent<DayNightCycle>();
            ApplyFields(cycle, so => SetObject(so, PresetFieldName, preset));

            var lighting = systems.AddComponent<DayNightLighting>();
            ApplyFields(lighting, so =>
            {
                SetObject(so, CycleFieldName, cycle);
                SetObject(so, SunLightFieldName, sun);
                SetObject(so, MoonLightFieldName, moon);
            });

            var sky = systems.AddComponent<DayNightSkybox>();
            ApplyFields(sky, so =>
            {
                SetObject(so, CycleFieldName, cycle);
                SetObject(so, SkyboxTemplateFieldName, skybox);
            });

            return cycle;
        }

        private static void CreateEnvironment()
        {
            GroundGridFactory.CreateGround(GroundName);

            var landmarks = new GameObject(LandmarksName);
            for (var i = 0; i < LandmarkCount; i++)
            {
                var angle = i / (float)LandmarkCount * FullTurnRadians;
                var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = $"{BlockNamePrefix}{i}";
                block.transform.SetParent(landmarks.transform, false);
                block.transform.position = new Vector3(Mathf.Cos(angle) * LandmarkRadius, LandmarkHeight, Mathf.Sin(angle) * LandmarkRadius);
                block.transform.localScale = LandmarkScale;
                block.isStatic = true;
            }
        }

        private static Light CreateDirectionalLight(string name, Color color, float intensity, float pitch)
        {
            var go = new GameObject(name);
            go.transform.rotation = Quaternion.Euler(pitch, LightYaw, 0f);

            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;

            return light;
        }

        private static Transform CreatePlayer(InputReader reader)
        {
            var player = new GameObject(PlayerName);
            player.transform.position = PlayerStartPosition;

            var controller = player.AddComponent<CharacterController>();
            controller.height = ControllerHeight;
            controller.radius = ControllerRadius;
            controller.center = ControllerCenter;
            controller.slopeLimit = ControllerSlopeLimit;
            controller.stepOffset = ControllerStepOffset;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = PlayerBodyName;
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = PlayerBodyPosition;
            body.transform.localScale = PlayerBodyScale;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var pivot = new GameObject(CameraPivotName);
            pivot.transform.SetParent(player.transform, false);
            pivot.transform.localPosition = CameraPivotPosition;

            var motor = player.AddComponent<PlayerMotor>();
            ApplyFields(motor, so => SetObject(so, InputReaderFieldName, reader));

            var look = player.AddComponent<PlayerLook>();
            ApplyFields(look, so =>
            {
                SetObject(so, InputReaderFieldName, reader);
                SetObject(so, CameraPivotFieldName, pivot.transform);
            });

            return pivot.transform;
        }

        #endregion

        #region User Interface

        private static void CreateUserInterface(InputReader reader, DayNightCycle cycle)
        {
            new GameObject(EventSystemName, typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvasGo = CreateCanvas(CanvasName).gameObject;

            var clock = CreateText(ClockName, canvasGo.transform, ClockFontSize, TextAlignmentOptions.TopLeft);
            AnchorCorner(clock.rectTransform, ClockAnchor, ClockOffset, ClockSize);

            var clockUI = canvasGo.AddComponent<ClockUI>();
            ApplyFields(clockUI, so =>
            {
                SetObject(so, CycleFieldName, cycle);
                SetObject(so, LabelFieldName, clock);
            });

            CreateFpsCounter(canvasGo.transform);

            var hint = CreateText(HintName, canvasGo.transform, HintFontSize, TextAlignmentOptions.BottomLeft);
            hint.text = HintText;
            AnchorCorner(hint.rectTransform, HintAnchor, HintOffset, HintSize);

            var panel = CreateRebindPanel(canvasGo.transform, reader, out var resetAllButton, out var closeButton);

            var menu = canvasGo.AddComponent<RebindMenuUI>();
            ApplyFields(menu, so =>
            {
                SetObject(so, InputReaderFieldName, reader);
                SetObject(so, PanelFieldName, panel.gameObject);
                SetObject(so, ResetAllButtonFieldName, resetAllButton);
                SetObject(so, CloseButtonFieldName, closeButton);
            });
        }

        private static RectTransform CreateRebindPanel(Transform parent, InputReader reader, out Button resetAllButton, out Button closeButton)
        {
            var panel = CreateUIObject(RebindPanelName, parent);
            panel.anchorMin = PanelAnchor;
            panel.anchorMax = PanelAnchor;
            panel.pivot = PanelAnchor;
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = PanelSize;

            var background = panel.gameObject.AddComponent<Image>();
            background.color = PanelBackgroundColor;

            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(PanelPadding, PanelPadding, PanelPadding, PanelPadding);
            layout.spacing = PanelSpacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var title = CreateText(PanelTitleName, panel, PanelTitleFontSize, TextAlignmentOptions.Center);
            title.text = PanelTitleText;
            AddLayoutElement(title.gameObject, NoPreferredWidth, PanelElementHeight);

            CreateRebindRows(panel, reader);

            resetAllButton = CreateButton(ResetAllText, panel, NoPreferredWidth, out var resetAllLabel);
            resetAllLabel.text = ResetAllText;
            AddLayoutElement(resetAllButton.gameObject, NoPreferredWidth, PanelElementHeight);

            closeButton = CreateButton(CloseText, panel, NoPreferredWidth, out var closeLabel);
            closeLabel.text = CloseText;
            AddLayoutElement(closeButton.gameObject, NoPreferredWidth, PanelElementHeight);

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

            CreateCompositeRow(parent, reader, map, InputReader.MoveActionName, MoveUpPartName, MoveForwardTitle);
            CreateCompositeRow(parent, reader, map, InputReader.MoveActionName, MoveDownPartName, MoveBackTitle);
            CreateCompositeRow(parent, reader, map, InputReader.MoveActionName, MoveLeftPartName, MoveLeftTitle);
            CreateCompositeRow(parent, reader, map, InputReader.MoveActionName, MoveRightPartName, MoveRightTitle);
            CreateSimpleRow(parent, reader, map, InputReader.JumpActionName, JumpTitle);
            CreateSimpleRow(parent, reader, map, InputReader.SprintActionName, SprintTitle);
            CreateSimpleRow(parent, reader, map, InputReader.InteractActionName, InteractTitle);
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

            var row = CreateUIObject($"{RowNamePrefix}{title}", parent);
            AddLayoutElement(row.gameObject, NoPreferredWidth, PanelElementHeight);

            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = RowSpacing;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;

            var label = CreateText(RowLabelName, row, RowLabelFontSize, TextAlignmentOptions.MidlineLeft);
            label.text = title;
            AddLayoutElement(label.gameObject, RowLabelWidth, RowLabelHeight);

            var rebindButton = CreateButton(RebindButtonName, row, RebindButtonWidth, out var bindingLabel);
            var resetButton = CreateButton(ResetText, row, ResetButtonWidth, out var resetLabel);
            resetLabel.text = ResetText;

            var component = row.gameObject.AddComponent<RebindButtonUI>();
            ApplyFields(component, so =>
            {
                SetObject(so, InputReaderFieldName, reader);
                SetString(so, ActionMapNameFieldName, mapName);
                SetString(so, ActionNameFieldName, actionName);
                SetInt(so, BindingIndexFieldName, bindingIndex);
                SetString(so, DisplayTitleFieldName, title);
                SetObject(so, TitleLabelFieldName, label);
                SetObject(so, BindingLabelFieldName, bindingLabel);
                SetObject(so, RebindButtonFieldName, rebindButton);
                SetObject(so, ResetButtonFieldName, resetButton);
            });
        }

        #endregion

        #region Binding Lookup

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

            return InvalidBindingIndex;
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

            return InvalidBindingIndex;
        }

        private static bool IsKeyboardBinding(InputBinding binding)
        {
            var path = binding.effectivePath;

            return !string.IsNullOrEmpty(path) && path.IndexOf(KeyboardDeviceToken, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion
    }
}
