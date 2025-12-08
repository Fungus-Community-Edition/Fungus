using Amanita.VScripting;
using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// Utility MonoBehaviour for debugging purposes in the Unity Editor.
    /// </summary>
    public static class DebugUtils
    {
        [MenuItem("Tools/Amanita/Debug/DangerZone/ResetAllUidsAndRegistries")]
        public static void ResetAllUidsAndRegistries()
        {
            Debug.Log($"Resetting all UIDs and GUID registries at {System.DateTime.Now}");

            var fcGuidRegistry = AmanitaManager.GetOrAddGuidRegistryFor<Flowchart>();//
            var vsaGuidRegistry = AmanitaManager.GetOrAddGuidRegistryFor<VariableSourceAsset>();
            fcGuidRegistry.Clear();
            vsaGuidRegistry.Clear();

            // Find all Flowcharts in the current scene and reset their GUIDs
            var allFlowcharts = Object.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
            foreach (var flowchart in allFlowcharts)
            {
                flowchart.ForceResetUid();
            }

            // Find All VariableSourceAssets in the project and reset their GUIDs
            var allVSAs = AssetDatabase.FindAssets("t:VariableSourceAsset");
            foreach (var vsaGuid in allVSAs)
            {
                var vsaPath = AssetDatabase.GUIDToAssetPath(vsaGuid);
                var vsa = AssetDatabase.LoadAssetAtPath<VariableSourceAsset>(vsaPath);
                if (vsa != null)
                {
                    vsa.ForceResetUid();
                    vsa.MarkDirtyAndSave();
                }
            }
        }
    }
}
