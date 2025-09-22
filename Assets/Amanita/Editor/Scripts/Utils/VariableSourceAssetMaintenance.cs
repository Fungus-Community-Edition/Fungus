using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// We need this to compensate for how SerializeReference doesn't always keep the state
    /// of things as they should be when Unity reloads assemblies. This makes sure that
    /// all MuscariableHolders inside VariableSourceAssets are properly re-linked to their
    /// corresponding Muscariables.
    /// </summary>
    public static class VariableSourceAssetMaintenance
    {
        // Allows tests to swap out filesystem/asset behavior.
        internal static IEditorAssetResolver AssetResolver { get; set; } = new DefaultEditorAssetResolver();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        public static void Init()
        {
            AssemblyReloadEvents.afterAssemblyReload -= RefreshVariableSourceAssets;
            AssemblyReloadEvents.afterAssemblyReload += RefreshVariableSourceAssets;

            VariableSourceAsset.AnyRightBeforeVarAdded -= OnRightBeforeAnyAssetAddVariable;
            VariableSourceAsset.AnyRightBeforeVarAdded += OnRightBeforeAnyAssetAddVariable;
        }

        public static void RefreshVariableSourceAssets()
        {
            // We only count the assets in a Resources folder
            Debug.Log("Calling RefreshVariableSourceAssets..."); 
            IList<VariableSourceAsset> allAssets = AssetResolver.LoadAllFromResources<VariableSourceAsset>("").ToList();
            AssetResolver.StartAssetEditing();
            foreach (var asset in allAssets)
            {
                asset.Refresh();
                FixMuscariableHolders();
                void FixMuscariableHolders()
                {

                    // We need to access the MuscariableHolders too, to make sure they get updated
                    var path = AssetResolver.GetAssetPath(asset);
                    var holders = AssetResolver.LoadAllAssetsAtPath<MuscariableHolder>(path);
                    foreach (var toFix in holders)
                    {
                        Muscariable realVar = asset.Variables.Where((varWeHave) => varWeHave.ItemID == toFix.ItemID)
                            .FirstOrDefault() as Muscariable;
                        if (realVar == null)
                        {
                            Debug.LogWarning($"VariableSourceAsset.Refresh: Found a MuscariableHolder (ItemID {toFix.ItemID}) " +
                                $"that doesn't correspond to any Muscariable in the VariableSourceAsset ({asset.name}).");
                            continue;
                        }
                        toFix.Init(realVar);
                        Debug.Log($"Fixed MuscariableHolder (ItemID {toFix.ItemID}) in VariableSourceAsset {asset.name}. " +
                            $"Its key: {toFix.Key}. Its value: {toFix.Value} Real " +
                            $"var's key: {realVar.Key}. Real var's value: {realVar.Value}");
                    }
                }
                Debug.Log($"Refreshed VariableSourceAsset {asset.name}");
            }
            AssetResolver.StopAssetEditing();
            AssetResolver.RefreshAssets(); 
            AssetsRefreshed();//
        }

        public static System.Action AssetsRefreshed = delegate { };

        private static void OnRightBeforeAnyAssetAddVariable(Muscariable muscari)
        {
            RegisterHolderUnderIt();
            void RegisterHolderUnderIt()
            {
                VariableSourceAsset source = muscari.Owner as VariableSourceAsset;
                var holder = ScriptableObject.CreateInstance<MuscariableHolder>();
                holder.Init(muscari);
                AssetResolver.AddObjectToAsset(holder, source);
                Debug.Log($"Registered new MuscariableHolder inside VariableSourceAsset {source.name}" +
                    $" (ItemID {muscari.ItemID}, Key {muscari.Key})");
            }
        }
    }
}