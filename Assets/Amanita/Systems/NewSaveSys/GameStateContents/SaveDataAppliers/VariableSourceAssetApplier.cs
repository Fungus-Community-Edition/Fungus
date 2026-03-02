using UnityEngine;
using System.Collections.Generic;
using AtMycelia.Amanita.VScripting;
using System.Linq;

namespace AtMycelia.SaveSys
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

        public override void Apply(VariableSourceAssetSaveData saveData)
        {
            VariableSourceAsset toApplyTo = variableSourceAssets.Where((elem) => elem.UniqueId == saveData.UniqueId).FirstOrDefault();
            if (toApplyTo == null)
            {
                Debug.LogWarning($"No VariableSourceAsset with AssetId {saveData.UniqueId} was found to apply save data to.");
                return;
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
                varEl ??= toApplyTo.GetVariableByName(varSaveData.VarName); // Fallback to searching by name

                if (varEl == null)
                {
                    Debug.LogWarning($"Variable {varSaveData.VarName} not found in flowchart {toApplyTo.name}.");
                    continue;
                }

                forThisVar.ApplyState(varEl, varSaveData);
            }

        }

        public override void Apply(SaveData saveData, System.Action onComplete)
        {
            Apply(saveData as VariableSourceAssetSaveData);
            onComplete?.Invoke();
        }
    }
}