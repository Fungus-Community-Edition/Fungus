using UnityEngine;
using System.IO;

namespace Amanita
{
    public static class SOUtils
    {
        public static T GetOrCreateScriptableObject<T>(string assetName, string resourcesFolderPath)
            where T : ScriptableObject
        {
            // Try to load from Resources
            var result = Resources.Load<T>($"GuidRegistries/{assetName}");
            if (result != null)
            {
                return result;
            }

            // Create new instance, making sure it's an asset at the requested path
            result = ScriptableObject.CreateInstance<T>();

#if UNITY_EDITOR
            string folderPath = Path.Combine("Assets/Resources", resourcesFolderPath);
            bool folderExists = UnityEditor.AssetDatabase.IsValidFolder(folderPath);
            if (!folderExists)
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", resourcesFolderPath);
            }
            string assetPath = Path.Combine(folderPath, assetName + ".asset");//
            UnityEditor.AssetDatabase.CreateAsset(result, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
            return result;
        }
    }
}