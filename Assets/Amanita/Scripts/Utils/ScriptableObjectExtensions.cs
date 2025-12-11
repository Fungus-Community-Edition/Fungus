using UnityEngine;
using UnityEditor;

namespace Amanita
{
    public static class ScriptableObjectExtensions
    {
        /// <summary>
        /// Marks the ScriptableObject as dirty and saves the asset database.
        /// </summary>
        public static void MarkDirtyAndSave(this ScriptableObject so)
        {
            // As editor-centric as this method is, we want this in the core assembly so that other classes can
            // call it without needing to create an editor assembly dependency. Given how Amanita's core
            // editor one depends on Amanita's core runtime one... yeah.
            EditorUtility.SetDirty(so);

            string path = AssetDatabase.GetAssetPath(so);
            bool soIsAssetInProject = !string.IsNullOrEmpty(path) && !AssetDatabase.IsSubAsset(so);
            if (soIsAssetInProject)
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