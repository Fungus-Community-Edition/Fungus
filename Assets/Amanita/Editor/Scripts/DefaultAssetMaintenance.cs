using UnityEditor;
using UnityEngine;
using Amanita.SaveSys;
using Amanita.Tweening;

namespace Amanita.EditorUtils
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
            EnsureSaveStorageSettings();
            EnsureDefaultTweenAdapter();
            EnsureDefaultEncryptor();
            EnsureDefaultDecryptor();
        }

        public static SaveStorageSettings EnsureSaveStorageSettings()
        {
            SaveStorageSettings settings = DefaultAmanitaAssets.SaveStorageSettings;
            if (settings == null)
            {
                string path = AmanitaConstants.PathToSaveSysDefaultsFolder;
                settings = SOUtils.GetOrCreateScriptableObject<SaveStorageSettings>(path, "DefaultSaveStorageSettings");
            }

            DefaultAmanitaAssets.SaveStorageSettings = settings;
            return settings;
        }

        public static DefaultTweenAdapter EnsureDefaultTweenAdapter()
        {
            DefaultTweenAdapter adaptor = DefaultAmanitaAssets.TweenAdapter;
            if (adaptor == null)
            {
                string pathToContainingFolder = string.Empty; // Relative to Resources
                adaptor = SOUtils.GetOrCreateScriptableObject<DefaultTweenAdapter>(pathToContainingFolder,
                    "DefaultTweenAdapter");
            }

            DefaultAmanitaAssets.TweenAdapter = adaptor;
            return adaptor;
        }

        private static string resourcesPath = "Assets/Amanita/Resources/";

        public static Encryptor EnsureDefaultEncryptor()
        {
            string path = AmanitaConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            var encryptor = SOUtils.GetOrCreateScriptableObject<Encryptor>(path, "DefaultEncryptor");
            DefaultAmanitaAssets.Encryptor = encryptor;
            return encryptor;
        }

        public static Decryptor EnsureDefaultDecryptor()
        {
            string path = AmanitaConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            var decryptor = SOUtils.GetOrCreateScriptableObject<Decryptor>(path, "DefaultDecryptor");
            DefaultAmanitaAssets.Decryptor = decryptor;
            return decryptor;
        }
    }
}