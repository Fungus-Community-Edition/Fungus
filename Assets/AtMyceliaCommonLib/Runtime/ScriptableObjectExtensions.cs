using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtMycelia
{
    public static class ScriptableObjectExtensions
    {
        /// <summary>
        /// [Editor-Only] Marks the ScriptableObject as dirty and saves the asset database. Make sure to only
        /// call this method on ScriptableObjects that are assets in the project, not ones that are
        /// only in memory. 
        /// </summary>
        public static void MarkDirtyAndSave(this ScriptableObject sObj)
        {
#if UNITY_EDITOR
            // As editor-centric as this method is, we want this in the core assembly so that other classes can
            // call it without needing to create an editor assembly dependency. Given how Amanita's core
            // editor one depends on Amanita's core runtime one... yeah.
            EditorUtility.SetDirty(sObj);
            AssetDatabase.SaveAssetIfDirty(sObj);
#else
            // In builds, we can't mark ScriptableObjects as dirty or save the asset database, so we can just
            // log a warning.
            Debug.LogWarning("MarkDirtyAndSave was called on a ScriptableObject in a build. This method is " +
            "editor-only and will not do anything in builds.");
#endif

        }

        /// <summary>
        /// [Editor-Only] Checks if the ScriptableObject is an asset in the project. This is important to check before
        /// </summary>
        /// <param name="sObj"></param>
        /// <returns></returns>
        public static bool IsAssetInProject(this ScriptableObject sObj)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(sObj);
            return !string.IsNullOrEmpty(path);
#else
            // In builds, we can only have ScriptableObjects that are in memory, so we
            // can just return false.
            return false;
#endif
        }
    }
}