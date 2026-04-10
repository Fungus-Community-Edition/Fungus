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

        private static void DoTheEnsuring()
        {
            Debug.Log($"Doing default asset maintenance...");
            EnsureDefaultTweenAdapter();
            EnsureVariableRegistryConfig();
        }

        public static DefaultTweenAdapter EnsureDefaultTweenAdapter()
        {
            DefaultTweenAdapter adaptor = DefaultHyphlowAssets.TweenAdapter;
            if (adaptor == null)
            {
                string pathToContainingFolder = "AtMycelia/Tweening"; // Relative to Resources
                adaptor = SOUtils.EnsureSOExists<DefaultTweenAdapter>(pathToContainingFolder,
                    "DefaultTweenAdapter");
            }

            DefaultHyphlowAssets.TweenAdapter = adaptor;
            return adaptor;
        }

        public static VariableRegistryConfig EnsureVariableRegistryConfig()
        {
            VariableRegistryConfig config = DefaultHyphlowAssets.VariableRegistryConfig;
            if (config == null)
            {
                string pathToContainingFolder = "AtMycelia/Amanita"; // Relative to Resources
                config = SOUtils.EnsureSOExists<VariableRegistryConfig>(pathToContainingFolder,
                    "VariableRegistryConfig");
            }

            DefaultHyphlowAssets.VariableRegistryConfig = config;
            return config;
        }
    }
}