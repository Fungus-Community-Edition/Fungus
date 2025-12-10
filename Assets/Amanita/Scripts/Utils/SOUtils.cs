using UnityEngine;
using System.IO;

namespace Amanita
{
    public static class SOUtils
    {
        public static T EnsureSOExists<T>(string resourcesSubfolderPath, string assetName)
            where T : ScriptableObject
        {
            // Try to load from Resources
            T result = (T)GetOrCreateScriptableObject(typeof(T), resourcesSubfolderPath, assetName);
            return result;
        }

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
    }
}