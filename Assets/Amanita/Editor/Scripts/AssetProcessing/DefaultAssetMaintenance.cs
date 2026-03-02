using UnityEditor;
using UnityEngine;
using AtMycelia.SaveSys;
using AtMycelia.Amanita.Tweening;

namespace AtMycelia.Amanita.EditorUtils
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

        }

        public static DefaultTweenAdapter EnsureDefaultTweenAdapter()
        {
            DefaultTweenAdapter adaptor = DefaultAmanitaAssets.TweenAdapter;
            if (adaptor == null)
            {
                string pathToContainingFolder = string.Empty; // Relative to Resources
                adaptor = SOUtils.EnsureSOExists<DefaultTweenAdapter>(pathToContainingFolder,
                    "DefaultTweenAdapter");
            }

            DefaultAmanitaAssets.TweenAdapter = adaptor;
            return adaptor;
        }

    }
}