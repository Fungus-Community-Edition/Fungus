using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtMycelia
{
    public static class SOUtils
    {
        /// <summary>
        /// Ensures that a ScriptableObject of type T exists at the specified Resources subfolder 
        /// path with the given asset name.
        /// </summary>
        public static T EnsureSOExists<T>(string resourcesSubfolderPath, string assetName)
            where T : ScriptableObject
        {
            // Try to load from Resources
            T result = (T)GetOrCreateScriptableObject(typeof(T), resourcesSubfolderPath, assetName);
            return result;
        }

        /// <summary>
        /// Gets or creates a ScriptableObject of the specified type at the given Resources subfolder path
        /// </summary>
        public static ScriptableObject GetOrCreateScriptableObject(System.Type soType, string resourcesSubfolderPath,
            string assetName)
        {
            // Try to load from Resources
            string fullPath = $"{resourcesSubfolderPath}/{assetName}";
            if (fullPath.StartsWith("/")) // For when resourcesSubfolderPath is empty
            {
                fullPath = fullPath.Substring(1);
            }
            var result = Resources.Load<ScriptableObject>(fullPath);
            if (result != null)
            {
                return result;
            }
            // Create new instance, making sure it's an asset at the requested path
            result = ScriptableObject.CreateInstance(soType);

#if UNITY_EDITOR
            string folderPath = Path.Combine("Assets/Resources", resourcesSubfolderPath);
            AssetUtils.EnsureFolderExists(folderPath);
            string assetPath = Path.Combine(folderPath, assetName + ".asset").Replace("\\", "/");

            UnityEditor.AssetDatabase.CreateAsset(result, assetPath);
            UnityEditor.EditorUtility.SetDirty(result);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(result);
            UnityEditor.AssetDatabase.Refresh();
#endif
            return result;

        }

        /// <summary>
        /// Finds the first asset of type T in the project. Returns null if none found. Editor-only method.
        /// </summary>
        public static T FindFirstInProject<T>() where T : ScriptableObject
        {
#if UNITY_EDITOR
            // Search for all assets of type T
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0)
                return null;

            // Load the first match
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
        Debug.LogError("FindFirstInProject<T>() can only be used in the Unity Editor.");
        return null;
#endif
        }

    }
}