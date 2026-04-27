#if UNITY_EDITOR
using UnityEditor;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [InitializeOnLoad]
    public static class VariableSourceAssetMigrationRunner
    {
        static VariableSourceAssetMigrationRunner()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private static void OnAfterAssemblyReload()
        {
            EditorApplication.delayCall += MigrateAllVariableSourceAssets;
        }

        private static void MigrateAllVariableSourceAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:VariableSourceAsset");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                VariableSourceAsset asset = AssetDatabase.LoadAssetAtPath<VariableSourceAsset>(path);
                if (asset != null)
                {
                    //asset.MigrateToVariableManager();
                }
            }
        }
    }
}
#endif