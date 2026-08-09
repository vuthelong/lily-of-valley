using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LilyOfValley.EditorTools
{
    public static class SerializedFieldUtility
    {
        #region Field Assignment

        public static void SetObject(SerializedObject serialized, string fieldName, Object value)
        {
            var property = FindField(serialized, fieldName);
            if (property == null) return;

            property.objectReferenceValue = value;
        }

        public static void SetString(SerializedObject serialized, string fieldName, string value)
        {
            var property = FindField(serialized, fieldName);
            if (property == null) return;

            property.stringValue = value;
        }

        public static void SetInt(SerializedObject serialized, string fieldName, int value)
        {
            var property = FindField(serialized, fieldName);
            if (property == null) return;

            property.intValue = value;
        }

        public static void SetFloat(SerializedObject serialized, string fieldName, float value)
        {
            var property = FindField(serialized, fieldName);
            if (property == null) return;

            property.floatValue = value;
        }

        #endregion

        #region Method

        public static void ApplyFields(Object target, Action<SerializedObject> edit)
        {
            var serialized = new SerializedObject(target);
            edit(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty FindField(SerializedObject serialized, string fieldName)
        {
            var property = serialized.FindProperty(fieldName);
            if (property == null) Debug.LogError($"{nameof(SerializedFieldUtility)}: field '{fieldName}' not found on {serialized.targetObject.GetType().Name}.");

            return property;
        }

        #endregion
    }
}
