using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amanita.Tweening
{
    public static class TweenAdapterUtility
    {
        private const string AssetName = "DefaultTweenAdapter";
        private const string ResourcesPath = "Assets/Resources/";

        public static DefaultTweenAdapter GetOrCreateDefaultAdapter()
        {
            // Try to load from Resources
            var adapter = Resources.Load<DefaultTweenAdapter>(AssetName);
            if (adapter != null)
                return adapter;

#if UNITY_EDITOR
            EnsureResourcesFolderIsThere();
            void EnsureResourcesFolderIsThere()
            {
                if (!AssetDatabase.IsValidFolder(ResourcesPath.TrimEnd('/')))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
            }

            // Create a new instance
            adapter = ScriptableObject.CreateInstance<DefaultTweenAdapter>();

            SaveIntoResourcesFolder();
            void SaveIntoResourcesFolder()
            {
                string assetPath = $"{ResourcesPath}{AssetName}.asset";
                AssetDatabase.CreateAsset(adapter, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created new {nameof(DefaultTweenAdapter)} at {assetPath}");
            }
#endif

            return adapter;
        }
    }
}