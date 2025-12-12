using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using Type = System.Type;

namespace Lorekeeper.EditorCode
{
    public class ShadowDatabaseMaintenance
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        public static void DiscoverAndRegister()
        {
            AssemblyReloadEvents.afterAssemblyReload -= Refresh;
            AssemblyReloadEvents.afterAssemblyReload += Refresh;
        }

        [MenuItem("Tools/Lorekeeper/Refresh Shadow Database", priority = 0)]
        private static void Refresh()
        {
            LKUtils.EnsureWeHaveResourcesFolder();
            settings = settingsFactory.GetSettings();

            ShadowDatabase database = LKUtils.GetShadowDatabase();

            IList<UnityObj> allAssets = GetAllAssetsInProject();
            static IList<UnityObj> GetAllAssetsInProject()
            {
                var allAssetGuids = AssetDatabase.FindAssets("");

                HashSet<string> allAssetPaths = new HashSet<string>();
                foreach (var guid in allAssetGuids)
                {
                    // Keep in mind the exclusions from settings

                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.StartsWith("Assets"))
                    {
                        continue;
                    }

                    foreach (var exclusion in settings.Blacklist)
                    {
                        bool moreThanJustASlash = exclusion.Length > 1;
                        if (moreThanJustASlash && path.StartsWith($"Assets{exclusion}"))
                        {
                            path = string.Empty;
                            break;
                        }
                    }
                    // Skip meta files, scene files, empty paths, and non-files.
                    bool emptyPath = string.IsNullOrEmpty(path);
                    bool isMetaFile = path.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase);
                    bool isSceneFile = path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase);
                    bool isFolder = AssetDatabase.IsValidFolder(path);//

                    if (emptyPath || isMetaFile || isSceneFile || isFolder)
                    {
                        continue;
                    }

                    allAssetPaths.Add(path);
                }

                List<UnityObj> allAssets = new List<UnityObj>();

                foreach (var path in allAssetPaths)
                {
                    var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);

                    foreach (var asset in assetsAtPath)
                    {
                        if (asset == null)
                            continue;

                        var type = asset.GetType();
                        bool isEditorOnly = type.Namespace != null && type.Namespace.StartsWith("UnityEditor");
                        bool isComponent = type.IsSubclassOf(typeof(Component));
                        bool isUnnamed = string.IsNullOrEmpty(asset.name) || asset.name == "(Clone)";
                        if (isEditorOnly || isComponent || isUnnamed)
                            continue;

                        // Explicitly skip known editor-only runtime-inaccessible types
                        if (type == monoScriptType ||
                            type.Name == "LightingSettings" ||
                            type.Name == "LightingDataAsset" ||
                            type.Name == "NavMeshData" ||
                            type.Name == "AssemblyDefinitionAsset" ||
                            type.Name == "GUISkin")
                        {
                            continue;
                        }

                        allAssets.Add(asset);
                    }
                }

                return allAssets;


            }

            RegisterAssetsInDatabase();
            void RegisterAssetsInDatabase()
            {
                database.Refresh(); // Need to make sure its dictionary is populated.
                foreach (var asset in allAssets)
                {
                    AssetType assetType = ShadowDatabase.GetAssetTypeFor(asset);
                    if (assetType != AssetType.Null)
                    {
                        database.TryAdd(asset, assetType, out _);
                    }
                }
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);
            AssetDatabase.Refresh();
            Debug.Log($"[ShadowDatabaseMaintenance.Refresh]: Shadow Database refreshed with " +
                $"{database.TotalAssetCount} assets.");
        }
    
        private static LorekeeperSettings settings;
        private static LorekeeperSettingsFactory settingsFactory = new LorekeeperSettingsFactory();
        private static readonly Type monoScriptType = typeof(MonoScript);

        [MenuItem("Tools/Lorekeeper/Clear Shadow Database")]
        private static void ClearDatabase()
        {
            ShadowDatabase database = LKUtils.GetShadowDatabase();
            database.ClearAllAssets();
        }
    }
}