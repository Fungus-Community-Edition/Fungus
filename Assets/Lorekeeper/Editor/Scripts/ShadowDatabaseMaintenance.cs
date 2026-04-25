using System;
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

                HashSet<string> allAssetPaths = GetValidAssetPaths();
                HashSet<string> GetValidAssetPaths()
                {
                    HashSet<string> result = new HashSet<string>();
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
                        bool isMetaFile = path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase);
                        bool isSceneFile = path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
                        bool isFolder = AssetDatabase.IsValidFolder(path);//

                        if (emptyPath || isMetaFile || isSceneFile || isFolder)
                        {
                            continue;
                        }

                        result.Add(path);
                    }

                    return result;
                }

                List<string> sortedAssetPaths = new List<string>(allAssetPaths);
                sortedAssetPaths.Sort(StringComparer.Ordinal);

                List<AssetEntry> allAssets = new List<AssetEntry>();

                foreach (var path in sortedAssetPaths)
                {
                    var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);

                    foreach (var asset in assetsAtPath)
                    {
                        if (asset == null)
                            continue;
                        GameObject assetAsGo = asset as GameObject;
                        if (assetAsGo != null && assetAsGo.transform.parent != null)
                        {
                            // Skip child GameObjects in prefab assets
                            continue;
                        }

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

                        var newEntry = AssetEntry.Create(asset, path);
                        allAssets.Add(newEntry);
                    }
                }

                allAssets.Sort(AssetEntryComparer);

                List<UnityObj> result = new List<UnityObj>(allAssets.Count);
                foreach (var entry in allAssets)
                {
                    result.Add(entry.Asset);
                }

                return result;
            }

            RegisterAssetsInDatabase();
            void RegisterAssetsInDatabase()
            {
                database.Refresh(); // Need to make sure its dictionary is populated.
                foreach (var asset in allAssets)
                {
                    if (IsAssetTypeDisabled(asset, out AssetType assetType))
                    {
                        continue;
                    }

                    database.TryAdd(asset, assetType, out _);
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

        private static bool IsAssetTypeDisabled(UnityObj asset, out AssetType assetType)
        {
            assetType = ShadowDatabase.GetAssetTypeFor(asset);
            if (assetType == AssetType.Null)
            {
                return true;
            }

            return !settings.IsAssetTypeEnabled(assetType);
        }

        private struct AssetEntry
        {
            public UnityObj Asset;
            public string Path;
            public long LocalId;
            public string TypeName;
            public string Name;

            public static AssetEntry Create(UnityObj asset, string path)
            {
                string guid;
                long localId = 0;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out guid, out localId))
                {
                    localId = 0;
                }

                return new AssetEntry
                {
                    Asset = asset,
                    Path = path,
                    LocalId = localId,
                    TypeName = asset.GetType().FullName ?? string.Empty,
                    Name = asset.name ?? string.Empty
                };
            }
        }

        private static readonly Comparison<AssetEntry> AssetEntryComparer = (left, right) =>
        {
            int pathCompare = StringComparer.Ordinal.Compare(left.Path, right.Path);
            if (pathCompare != 0)
            {
                return pathCompare;
            }

            int idCompare = left.LocalId.CompareTo(right.LocalId);
            if (idCompare != 0)
            {
                return idCompare;
            }

            int typeCompare = StringComparer.Ordinal.Compare(left.TypeName, right.TypeName);
            if (typeCompare != 0)
            {
                return typeCompare;
            }

            return StringComparer.Ordinal.Compare(left.Name, right.Name);
        };
    }
}