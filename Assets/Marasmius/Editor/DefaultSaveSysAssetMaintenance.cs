using UnityEditor;
using UnityEngine;

namespace AtMycelia.SaveSys.EditorUtils
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
            Debug.Log($"Doing default save sys asset maintenance...");

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
            SaveStorageSettings settings = DefaultSaveSysAssets.SaveStorageSettings;
            if (settings == null)
            {
                string path = SaveSysConstants.PathToSaveSysDefaultsFolder;
                settings = SOUtils.EnsureSOExists<SaveStorageSettings>(path, "Gen_DefSaveStorageSettings");
            }

            DefaultSaveSysAssets.SaveStorageSettings = settings;
            return settings;
        }

        public static SaveReader EnsureSaveReader()
        {
            string path = SaveSysConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            SaveReader reader = DefaultSaveSysAssets.SaveReader;
            if (reader == null)
            {
                reader = SOUtils.EnsureSOExists<SaveReader>(path, "Gen_DefSaveReader");
            }

            reader.StorageSettings = DefaultSaveSysAssets.SaveStorageSettings;
            reader.Decryptor = DefaultSaveSysAssets.Decryptor;
            DefaultSaveSysAssets.SaveReader = reader;
            return reader;
        }

        public static SaveWriter EnsureSaveWriter()
        {
            string path = SaveSysConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            SaveWriter writer = DefaultSaveSysAssets.SaveWriter;
            if (writer == null)
            {
                writer = SOUtils.EnsureSOExists<SaveWriter>(path, "Gen_DefSaveWriter");
            }

            writer.StorageSettings = DefaultSaveSysAssets.SaveStorageSettings;
            writer.Encryptor = DefaultSaveSysAssets.Encryptor;

            DefaultSaveSysAssets.SaveWriter = writer;
            return writer;
        }

        public static Encryptor EnsureDefaultEncryptor()
        {
            string path = SaveSysConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            var encryptor = SOUtils.EnsureSOExists<Encryptor>(path, "Gen_DefEncryptor");
            DefaultSaveSysAssets.Encryptor = encryptor;
            return encryptor;
        }

        public static Decryptor EnsureDefaultDecryptor()
        {
            string path = SaveSysConstants.PathToSaveSysDefaultsFolder; // Relative to Resources
            var decryptor = SOUtils.EnsureSOExists<Decryptor>(path, "Gen_DefDecryptor");
            DefaultSaveSysAssets.Decryptor = decryptor;
            return decryptor;
        }


    }
}