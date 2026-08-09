using System.IO;
using UnityEditor;
using UnityEngine;

namespace LilyOfValley.EditorTools
{
    public static class EditorAssetUtility
    {
        #region Public Methods
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent)) return;

            parent = parent.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
        #endregion
    }
}
