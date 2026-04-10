using AtMycelia;
using AtMycelia.Hyphlow.EditorUtils;
using AtMycelia.Hyphlow.Sys;
using UnityEditor;
using UnityEngine;

namespace Hyphlow.Editor
{
    /// <summary>
    /// For ensuring that certain editor-only assets are created and maintained, such as the HyphlowEditorSysAssets asset.
    /// </summary>
    public static class DefaultEditorAssetMaintenance 
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        public static void Init()
        {
            AssemblyReloadEvents.afterAssemblyReload -= DoTheEnsuring;
            AssemblyReloadEvents.afterAssemblyReload += DoTheEnsuring;
        }

        private static void DoTheEnsuring()
        {
            Debug.Log($"Doing default editor asset maintenance...");
            EnsureHyphlowEditorResourcesAsset();
            EnsureFcWindowConfig();
        }

        private static HyphlowEditorSysAssets EnsureHyphlowEditorResourcesAsset()
        {
            HyphlowEditorSysAssets assets = HyphlowEditorSysAssets.S;
            if (assets == null)
            {
                var all = Resources.LoadAll<HyphlowEditorSysAssets>("");
                if (all.Length > 0)
                {
                    assets = all[0];
                }
            }

            if (assets == null)
            {
                string pathToContainingFolder = "AtMycelia/Editor/Hyphlow/Sys"; // Relative to Resources
                assets = SOUtils.EnsureSOExists<HyphlowEditorSysAssets>(pathToContainingFolder,
                    "HyphlowEditorSysAssets");
            }

            return assets;

        }

        private static FlowchartWindowConfig EnsureFcWindowConfig()
        {
            FlowchartWindowConfig config = HyphlowEditorSysAssets.FcwConfig;
            if (config == null)
            {
                var all = Resources.LoadAll<FlowchartWindowConfig>("");
                if (all.Length > 0)
                {
                    config = all[0];
                }
            }

            if (config == null)
            {
                string pathToContainingFolder = "AtMycelia/Editor/"; // Relative to Resources
                config = SOUtils.EnsureSOExists<FlowchartWindowConfig>(pathToContainingFolder,
                    "FlowchartWindowConfig");
            }

            return config;

        }
    }
}