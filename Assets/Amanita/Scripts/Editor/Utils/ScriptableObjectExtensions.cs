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
            AssetDatabase.SaveAssetIfDirty(so);
        }
    }
}