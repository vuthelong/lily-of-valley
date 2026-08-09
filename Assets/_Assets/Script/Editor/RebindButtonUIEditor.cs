using LilyOfValley.Inputs;
using LilyOfValley.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LilyOfValley.EditorTools
{
    [CustomEditor(typeof(RebindButtonUI))]
    public sealed class RebindButtonUIEditor : Editor
    {
        #region Fields
        private static readonly string[] HandledProperties =
        {
            "m_Script", "inputReader", "actionMapName", "actionName", "bindingIndex"
        };
        #endregion

        #region Public Methods
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var readerProperty = serializedObject.FindProperty("inputReader");
            EditorGUILayout.PropertyField(readerProperty);

            var reader = readerProperty.objectReferenceValue as InputReader;
            var asset = reader != null ? reader.Actions : null;

            if (asset == null) EditorGUILayout.HelpBox($"Assign an {nameof(InputReader)} that holds an {nameof(InputActionAsset)} to pick a binding.", MessageType.Info);
            else DrawBindingSelectors(asset);

            DrawRemainingProperties();
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Private Methods
        private void DrawBindingSelectors(InputActionAsset asset)
        {
            var mapProperty = serializedObject.FindProperty("actionMapName");
            var actionProperty = serializedObject.FindProperty("actionName");
            var indexProperty = serializedObject.FindProperty("bindingIndex");

            var maps = asset.actionMaps;
            if (maps.Count == 0) return;

            var mapNames = new string[maps.Count];
            for (var i = 0; i < maps.Count; i++) mapNames[i] = maps[i].name;

            var mapIndex = Mathf.Max(0, IndexOf(mapNames, mapProperty.stringValue));
            mapIndex = EditorGUILayout.Popup("Action Map", mapIndex, mapNames);
            mapProperty.stringValue = mapNames[mapIndex];

            var actions = maps[mapIndex].actions;
            if (actions.Count == 0) return;

            var actionNames = new string[actions.Count];
            for (var i = 0; i < actions.Count; i++) actionNames[i] = actions[i].name;

            var actionIndex = Mathf.Max(0, IndexOf(actionNames, actionProperty.stringValue));
            actionIndex = EditorGUILayout.Popup("Action", actionIndex, actionNames);
            actionProperty.stringValue = actionNames[actionIndex];

            var action = actions[actionIndex];
            if (action.bindings.Count == 0) return;

            var bindingLabels = new string[action.bindings.Count];
            for (var i = 0; i < action.bindings.Count; i++) bindingLabels[i] = BuildBindingLabel(action.bindings[i], i);

            var bindingIndex = Mathf.Clamp(indexProperty.intValue, 0, bindingLabels.Length - 1);
            indexProperty.intValue = EditorGUILayout.Popup("Binding", bindingIndex, bindingLabels);

            if (!action.bindings[indexProperty.intValue].isComposite) return;

            EditorGUILayout.HelpBox("This is a composite head and cannot be rebound. Pick one of its parts (up / down / left / right).", MessageType.Warning);
        }

        private void DrawRemainingProperties()
        {
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (IsHandled(iterator.name)) continue;

                EditorGUILayout.PropertyField(iterator, true);
            }
        }

        private static string BuildBindingLabel(InputBinding binding, int index)
        {
            var display = binding.isComposite
                ? binding.name
                : InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            var part = binding.isPartOfComposite ? $"{binding.name}: " : string.Empty;
            var prefix = binding.isComposite ? "[composite] " : string.Empty;
            return $"{index} - {prefix}{part}{display}".Replace('/', '\\');
        }

        private static bool IsHandled(string propertyName)
        {
            for (var i = 0; i < HandledProperties.Length; i++)
            {
                if (HandledProperties[i] == propertyName) return true;
            }

            return false;
        }

        private static int IndexOf(string[] values, string value)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (values[i] == value) return i;
            }

            return -1;
        }
        #endregion
    }
}
