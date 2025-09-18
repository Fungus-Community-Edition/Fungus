using UnityEngine;

namespace Amanita.EditorUtils
{
    public static class ScriptableObjectExtensions
    {
        /// <summary>
        /// Marks the ScriptableObject as dirty and saves the asset database.
        /// </summary>
        public static void MarkDirtyAndSave(this ScriptableObject so)
        {
            UnityEditor.EditorUtility.SetDirty(so);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
}