// Asset save hook
using Amanita.VScripting;
using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// Watches for asset save events and ensures that any ScriptableObject implementing IHasUniqueID
    /// is properly registered in the GuidRegistry.
    /// </summary>
    class UniqueIdSaveWatcher : AssetModificationProcessor
    {
        // Even if the below is grayed out, don't worry; Unity uses reflection to find and call this method.
        static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset is IHasUniqueID uidSO)
                {
                    foreach (var guid in AssetDatabase.FindAssets("t:GuidRegistry"))
                    {
                        var registryPath = AssetDatabase.GUIDToAssetPath(guid);
                        var registry = AssetDatabase.LoadAssetAtPath<GuidRegistry>(registryPath);
                        if (registry == null)
                        {
                            continue;
                        }
                        registry.RegisterUidOf(uidSO);
                        registry.MarkDirtyAndSave();
                    }
                }
            }
            return paths;
        }
    }
}