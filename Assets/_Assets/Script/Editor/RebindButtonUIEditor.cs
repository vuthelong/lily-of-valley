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
        #region Field

        private const string ScriptPropertyName = "m_Script";
        private const string InputReaderPropertyName = "inputReader";
        private const string ActionMapPropertyName = "actionMapName";
        private const string ActionPropertyName = "actionName";
        private const string BindingIndexPropertyName = "bindingIndex";

        private const string ActionMapPopupLabel = "Action Map";
        private const string ActionPopupLabel = "Action";
        private const string BindingPopupLabel = "Binding";

        private const string MissingAssetMessage = "Assign an " + nameof(InputReader) + " that holds an " + nameof(InputActionAsset) + " to pick a binding.";
        private const string CompositeHeadMessage = "This is a composite head and cannot be rebound. Pick one of its parts (up / down / left / right).";

        private const string CompositeLabelPrefix = "[composite] ";
        private const string PartNameSeparator = ": ";
        private const string IndexSeparator = " - ";
        private const char BindingPathSeparator = '/';
        private const char BindingPathDisplaySeparator = '\\';

        private static readonly string[] HandledProperties =
        {
            ScriptPropertyName,
            InputReaderPropertyName,
            ActionMapPropertyName,
            ActionPropertyName,
            BindingIndexPropertyName
        };

        #endregion

        #region Unity Lifecycle

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var readerProperty = serializedObject.FindProperty(InputReaderPropertyName);
            EditorGUILayout.PropertyField(readerProperty);

            DrawBindingSelectors(readerProperty);
            DrawRemainingProperties();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Drawing

        private void DrawBindingSelectors(SerializedProperty readerProperty)
        {
            var reader = readerProperty.objectReferenceValue as InputReader;
            var asset = reader != null ? reader.Actions : null;
            if (asset == null)
            {
                EditorGUILayout.HelpBox(MissingAssetMessage, MessageType.Info);
                return;
            }

            var map = DrawMapPopup(asset);
            if (map == null) return;

            var action = DrawActionPopup(map);
            if (action == null) return;

            DrawBindingPopup(action);
        }

        private InputActionMap DrawMapPopup(InputActionAsset asset)
        {
            var maps = asset.actionMaps;
            if (maps.Count == 0) return null;

            var mapNames = new string[maps.Count];
            for (var i = 0; i < maps.Count; i++)
            {
                mapNames[i] = maps[i].name;
            }

            var mapProperty = serializedObject.FindProperty(ActionMapPropertyName);
            var mapIndex = Mathf.Max(0, IndexOf(mapNames, mapProperty.stringValue));
            mapIndex = EditorGUILayout.Popup(ActionMapPopupLabel, mapIndex, mapNames);
            mapProperty.stringValue = mapNames[mapIndex];

            return maps[mapIndex];
        }

        private InputAction DrawActionPopup(InputActionMap map)
        {
            var actions = map.actions;
            if (actions.Count == 0) return null;

            var actionNames = new string[actions.Count];
            for (var i = 0; i < actions.Count; i++)
            {
                actionNames[i] = actions[i].name;
            }

            var actionProperty = serializedObject.FindProperty(ActionPropertyName);
            var actionIndex = Mathf.Max(0, IndexOf(actionNames, actionProperty.stringValue));
            actionIndex = EditorGUILayout.Popup(ActionPopupLabel, actionIndex, actionNames);
            actionProperty.stringValue = actionNames[actionIndex];

            return actions[actionIndex];
        }

        private void DrawBindingPopup(InputAction action)
        {
            var bindings = action.bindings;
            if (bindings.Count == 0) return;

            var bindingLabels = new string[bindings.Count];
            for (var i = 0; i < bindings.Count; i++)
            {
                bindingLabels[i] = BuildBindingLabel(bindings[i], i);
            }

            var indexProperty = serializedObject.FindProperty(BindingIndexPropertyName);
            var bindingIndex = Mathf.Clamp(indexProperty.intValue, 0, bindingLabels.Length - 1);
            indexProperty.intValue = EditorGUILayout.Popup(BindingPopupLabel, bindingIndex, bindingLabels);

            if (!bindings[indexProperty.intValue].isComposite) return;

            EditorGUILayout.HelpBox(CompositeHeadMessage, MessageType.Warning);
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
            var part = binding.isPartOfComposite ? binding.name + PartNameSeparator : string.Empty;
            var prefix = binding.isComposite ? CompositeLabelPrefix : string.Empty;

            return $"{index}{IndexSeparator}{prefix}{part}{display}".Replace(BindingPathSeparator, BindingPathDisplaySeparator);
        }

        #endregion

        #region Name Lookup

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
