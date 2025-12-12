using UnityEngine;
using UnityEditor;

namespace Amanita
{
    public static class ScriptableObjectExtensions
    {
        /// <summary>
        /// Marks the ScriptableObject as dirty and saves the asset database. Make sure to only
        /// call this method on ScriptableObjects that are assets in the project, not ones that are
        /// only in memory.
        /// </summary>
        public static void MarkDirtyAndSave(this ScriptableObject sObj)
        {
            // As editor-centric as this method is, we want this in the core assembly so that other classes can
            // call it without needing to create an editor assembly dependency. Given how Amanita's core
            // editor one depends on Amanita's core runtime one... yeah.
            EditorUtility.SetDirty(sObj);
            AssetDatabase.SaveAssetIfDirty(sObj);

        }

        public static bool IsAssetInProject(this ScriptableObject sObj)
        {
            var path = AssetDatabase.GetAssetPath(sObj);
            return !string.IsNullOrEmpty(path);
        }
    }
}