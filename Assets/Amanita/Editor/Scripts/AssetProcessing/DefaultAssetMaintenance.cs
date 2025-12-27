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
            EnsureDefaultTweenAdapter();

            EnsureSaveStorageSettings();
            EnsureDefaultEncryptor();
            EnsureDefaultDecryptor();
            EnsureSaveReader();
            EnsureSaveWriter();
        }

        // We have these separate Ensure methods in case the user wants to call them individually
        // or during runtime.
        public static SaveStorageSettings EnsureSaveStorageSettings()
        {
            SaveStorageSettings settings = DefaultAmanitaAssets.SaveStorageSettings;
            if (settings == null)
            {
                string path = AmanitaConstants.PathToSaveSysDefaultsFolder;
                settings = SOUtils.EnsureSOExists<SaveStorageSettings>(path, "DefaultSaveStorageSettings");
            }

            DefaultAmanitaAssets.SaveStorageSettings = settings;
            return settings;
        }

        public static SaveReader EnsureSaveReader()
        {
            string path = AmanitaConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            SaveReader reader = DefaultAmanitaAssets.SaveReader;
            if (reader == null)
            {
                reader = SOUtils.EnsureSOExists<SaveReader>(path, "DefaultSaveReader");
            }

            reader.StorageSettings = DefaultAmanitaAssets.SaveStorageSettings;
            reader.Decryptor = DefaultAmanitaAssets.Decryptor;
            DefaultAmanitaAssets.SaveReader = reader;
            return reader;
        }

        public static SaveWriter EnsureSaveWriter()
        {
            string path = AmanitaConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            SaveWriter writer = DefaultAmanitaAssets.SaveWriter;
            if (writer == null)
            {
                writer = SOUtils.EnsureSOExists<SaveWriter>(path, "DefaultSaveWriter");
            }

            writer.StorageSettings = DefaultAmanitaAssets.SaveStorageSettings;
            writer.Encryptor = DefaultAmanitaAssets.Encryptor;

            DefaultAmanitaAssets.SaveWriter = writer;
            return writer;
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

        

        public static Encryptor EnsureDefaultEncryptor()
        {
            string path = AmanitaConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            var encryptor = SOUtils.EnsureSOExists<Encryptor>(path, "DefaultEncryptor");
            DefaultAmanitaAssets.Encryptor = encryptor;
            return encryptor;
        }

        public static Decryptor EnsureDefaultDecryptor()
        {
            string path = AmanitaConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            var decryptor = SOUtils.EnsureSOExists<Decryptor>(path, "DefaultDecryptor");
            DefaultAmanitaAssets.Decryptor = decryptor;
            return decryptor;
        }


    }
}