using Amanita.EditorUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        public static void Init()
        {
            AssemblyReloadEvents.afterAssemblyReload -= RefreshVariableSourceAssets;
            AssemblyReloadEvents.afterAssemblyReload += RefreshVariableSourceAssets;

            VariableSourceAsset.AnyRightBeforeVarAdded -= OnRightBeforeAnyAssetAddVariable;
            VariableSourceAsset.AnyRightBeforeVarAdded += OnRightBeforeAnyAssetAddVariable;
        }

        private static void RefreshVariableSourceAssets()
        {
            // We only count the assets in a Resources folder
            IList<VariableSourceAsset> allAssets = Resources.LoadAll<VariableSourceAsset>("");
            AssetDatabase.StartAssetEditing();
            foreach (var asset in allAssets)
            {
                asset.Refresh();
                FixMuscariableHolders();
                void FixMuscariableHolders()
                {
                    // We need to access the MuscariableHolders too, to make sure they get updated
                    var holders = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset)).OfType<MuscariableHolder>();
                    foreach (var toFix in holders)
                    {
                        Muscariable realVar = asset.Variables.Where((varWeHave) => varWeHave.ItemID == toFix.ItemID)
                            .FirstOrDefault() as Muscariable; //
                        if (realVar == null)
                        {
                            // There is a serious problem. Best log it.
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
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
    
        private static void OnRightBeforeAnyAssetAddVariable(Muscariable muscari)
        {
            RegisterHolderUnderIt();
            void RegisterHolderUnderIt()
            {
                VariableSourceAsset source = muscari.Owner as VariableSourceAsset;
                var holder = ScriptableObject.CreateInstance<MuscariableHolder>();
                holder.Init(muscari);
                AssetDatabase.AddObjectToAsset(holder, source);
                //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(source));
                Debug.Log($"Registered new MuscariableHolder inside VariableSourceAsset {source.name}" +
                    $" (ItemID {muscari.ItemID}, Key {muscari.Key})");
            }
        }
    }
}