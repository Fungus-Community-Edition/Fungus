using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using Amanita.VScripting;
using System.Linq;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This is meant to apply to VariableSourceAssets on disk.
    /// </summary>
    [SaveSysDisplayName("Var Source Asset Applier (Amanita Default)")]
    public class VariableSourceAssetApplier : SaveDataApplier<VariableSourceAssetSaveData>
    {
        public override void PreInstallInit()
        {
            base.PreInstallInit();
            variableSourceAssets = Resources.LoadAll<VariableSourceAsset>("");
            // ^Best to grab all these in init so we don't have to do it repeatedly later.
        }

        protected IList<VariableSourceAsset> variableSourceAssets;

        public override Task Apply(VariableSourceAssetSaveData saveData)
        {
            VariableSourceAsset toApplyTo = variableSourceAssets.Where((elem) => elem.UniqueId == saveData.UniqueId).FirstOrDefault();
            if (toApplyTo == null)
            {
                Debug.LogWarning($"No VariableSourceAsset with AssetId {saveData.UniqueId} was found to apply save data to.");
                return Task.CompletedTask;
            }

            foreach (VariableSaveData varSaveData in saveData.SavedVars)
            {
                IVarCodec forThisVar = VarCodecRegistry.GetCodec(varSaveData);
                if (forThisVar == null)
                {
                    Debug.LogWarning($"No codec found for variable type: {varSaveData.VarTypeName}");
                    continue;
                }

                IVariable varEl = toApplyTo.GetVariable(varSaveData.ItemId);
                varEl ??= toApplyTo.GetVariable(varSaveData.VarName); // Fallback to searching by name

                if (varEl == null)
                {
                    Debug.LogWarning($"Variable {varSaveData.VarName} not found in flowchart {toApplyTo.name}.");
                    continue;
                }

                forThisVar.ApplyState(varEl, varSaveData);
            }

            return Task.CompletedTask;
        }

        public override Task Apply(SaveData saveData)
        {
            return Apply(saveData as VariableSourceAssetSaveData);
        }
    }
}