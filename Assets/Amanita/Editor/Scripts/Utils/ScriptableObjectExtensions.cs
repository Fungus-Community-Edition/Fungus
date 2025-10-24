using UnityEngine;
using UnityEditor;

namespace Amanita.EditorUtils
{
    public static class ScriptableObjectExtensions
    {
        /// <summary>
        /// Marks the ScriptableObject as dirty and saves the asset database.
        /// </summary>
        public static void MarkDirtyAndSave(this ScriptableObject so)
        {
            EditorUtility.SetDirty(so);

            // Ensure the asset file itself is marked dirty
            string path = AssetDatabase.GetAssetPath(so);
            if (!string.IsNullOrEmpty(path))
            {
                var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                if (mainAsset != null)
                {
                    EditorUtility.SetDirty(mainAsset);
                    Debug.Log($"Asset file for {so.name} marked dirty at path: {path}");
                }
            }

            AssetDatabase.SaveAssetIfDirty(so);

        }
    }
}