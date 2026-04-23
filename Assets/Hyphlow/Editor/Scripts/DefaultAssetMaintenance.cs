using AtMycelia.Hyphlow.Sys;
using UnityEditor;
using UnityEngine;
using AtMycelia.Hyphlow.Tweening;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// For ensuring that certain default assets are present in the project.
    /// </summary>
    public static class DefaultAssetMaintenance 
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        public static void Init()
        {
            AssemblyReloadEvents.afterAssemblyReload -= DoTheEnsuring;
            AssemblyReloadEvents.afterAssemblyReload += DoTheEnsuring;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitInEditor()
        {
            // This is to help make sure that the Singletons aren't lost for too long.
#if UNITY_EDITOR
            DoTheEnsuring();
#endif
        }

        private static void DoTheEnsuring()
        {
            Debug.Log($"Doing default asset maintenance...");
            EnsureHyphlowRuntimeSysAssets();
            EnsureDefaultTweenAdapter();
            EnsureVariableRegistryConfig();
        }

        public static HyphlowRuntimeSysAssets EnsureHyphlowRuntimeSysAssets()
        {
            HyphlowRuntimeSysAssets assets = HyphlowRuntimeSysAssets.S;

            if (assets == null)
            {
                var all = Resources.LoadAll<HyphlowRuntimeSysAssets>("");
                if (all.Length > 0)
                {
                    assets = all[0];
                }
            }

            if (assets == null)
            {
                string pathToContainingFolder = "AtMycelia/Hyphlow/Sys"; // Relative to Resources
                assets = SOUtils.EnsureSOExists<HyphlowRuntimeSysAssets>(pathToContainingFolder,
                    "HyphlowRuntimeSysAssets");
            }
            HyphlowRuntimeSysAssets.S = assets;
            return assets;
        }
        public static DefaultTweenAdapter EnsureDefaultTweenAdapter()
        {
            DefaultTweenAdapter adaptor = HyphlowRuntimeSysAssets.S.TweenAdapter;
            if (adaptor == null)
            {
                string pathToContainingFolder = "AtMycelia/Hyphlow/Sys"; // Relative to Resources
                adaptor = SOUtils.EnsureSOExists<DefaultTweenAdapter>(pathToContainingFolder,
                    "DefaultTweenAdapter");
            }

            HyphlowRuntimeSysAssets.S.TweenAdapter = adaptor;
            return adaptor;
        }

        public static VariableRegistryConfig EnsureVariableRegistryConfig()
        {
            VariableRegistryConfig config = HyphlowRuntimeSysAssets.S.VariableRegistryConfig;
            if (config == null)
            {
                string pathToContainingFolder = "AtMycelia/Hyphlow/Sys"; // Relative to Resources
                config = SOUtils.EnsureSOExists<VariableRegistryConfig>(pathToContainingFolder,
                    "VariableRegistryConfig");
            }

            HyphlowRuntimeSysAssets.S.VariableRegistryConfig = config;
            return config;
        }
    }
}