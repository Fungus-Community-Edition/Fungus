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
            if (DefaultAmanitaAssets.SaveStorageSettings != null)
            {
                return DefaultAmanitaAssets.SaveStorageSettings; // All good
            }
            string pathToDefault = AmanitaConstants.PathToDefaultSaveStorageSettings;
            var settings = Resources.Load<SaveStorageSettings>(pathToDefault);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<SaveStorageSettings>();
                string pathToAsset = $"{saveSysResourcesPath}DefaultSaveStorageSettings.asset";
                AssetDatabase.CreateAsset(settings, pathToAsset);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created default SaveStorageSettings at {pathToAsset}");
            }
            
            DefaultAmanitaAssets.SaveStorageSettings = settings;
            return settings;

        }

        public static DefaultTweenAdapter EnsureDefaultTweenAdapter()
        {
            if (DefaultAmanitaAssets.TweenAdapter != null)
            {
                return DefaultAmanitaAssets.TweenAdapter; // All good
            }

            string pathToAdapter = AmanitaConstants.PathToDefaultTweenAdapter; // Relative to Resources
            var adapter = Resources.Load<DefaultTweenAdapter>(pathToAdapter);
            if (adapter == null)
            {
                // This will create it in Resources if not there
                adapter = ScriptableObject.CreateInstance<DefaultTweenAdapter>();
                adapter.name = "DefaultTweenAdapter";
                string pathToAsset = $"{resourcesPath}{adapter.name}.asset";
                AssetDatabase.CreateAsset(adapter, pathToAsset);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created default DefaultTweenAdapter at {pathToAsset}");
            }

            DefaultAmanitaAssets.TweenAdapter = adapter;
            return adapter;
        }

        private static string resourcesPath = "Assets/Amanita/Resources/";
        private static string saveSysResourcesPath = resourcesPath + "SaveSys/";

        public static Encryptor EnsureDefaultEncryptor()
        {
            string pathToEncryptor = AmanitaConstants.PathToDefaultEncryptor; // Relative to Resources
            var encryptor = Resources.Load<Encryptor>(pathToEncryptor);
            if (encryptor == null)
            {
                // This will create it in Resources if not there
                encryptor = ScriptableObject.CreateInstance<Encryptor>();
                encryptor.name = "DefaultEncryptor";
                string pathToAsset = $"{saveSysResourcesPath}{encryptor.name}.asset";
                AssetDatabase.CreateAsset(encryptor, pathToAsset);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created default Encryptor at {pathToAsset}");
            }

            DefaultAmanitaAssets.Encryptor = encryptor;
            return encryptor;
        }

        public static Decryptor EnsureDefaultDecryptor()
        {
            string pathToDecryptor = AmanitaConstants.PathToDefaultDecryptor; // Relative to Resources
            var decryptor = Resources.Load<Decryptor>(pathToDecryptor);
            if (decryptor == null)
            {
                // This will create it in Resources if not there
                decryptor = ScriptableObject.CreateInstance<Decryptor>();
                decryptor.name = "DefaultDecryptor";
                string pathToAsset = $"{saveSysResourcesPath}{decryptor.name}.asset";
                AssetDatabase.CreateAsset(decryptor, pathToAsset);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created default Decryptor at {pathToAsset}");
            }

            DefaultAmanitaAssets.Decryptor = decryptor;
            return decryptor;
        }
    }
}