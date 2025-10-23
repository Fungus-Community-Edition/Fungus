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
    public class VariableSourceAssetApplier : SaveDataApplier<VariableSourceAssetSaveData>
    {
        [SerializeField] protected ScriptableObject[] varCodecs = new ScriptableObject[0];

        public virtual void RegisterVarCodec(IVarCodec codec)
        {
            if (codec == null)
            {
                Debug.LogError("Cannot register a null codec.");
                return;
            }
            if (validCodecs.Contains(codec))
            {
                Debug.LogWarning($"Codec {codec.GetType().Name} is already registered.");
                return;
            }
            validCodecs.Add(codec);
        }

        protected IList<IVarCodec> validCodecs = new List<IVarCodec>();

        protected virtual void OnEnable()
        {
            RefreshValidCodecs();
        }

        protected virtual void RefreshValidCodecs()
        {
            validCodecs.Clear();
            for (int i = 0; i < varCodecs.Length; i++)
            {
                ScriptableObject toCheck = varCodecs[i];
                if (toCheck is not IVarCodec && toCheck != null)
                {
                    string name = toCheck.name;
                    Debug.LogError($"Element at index {i} ({name}) in varCodecs is not an IVarCodec. " +
                        $"Please fix this.");
                }
                else if (toCheck is IVarCodec codecFound)
                {
                    validCodecs.Add(codecFound);
                }
            }
        }

        public override void Init()
        {
            base.Init();
            variableSourceAssets = Resources.LoadAll<VariableSourceAsset>("");
        }

        protected IList<VariableSourceAsset> variableSourceAssets;

        public override Task Apply(VariableSourceAssetSaveData saveData)
        {
            VariableSourceAsset toApplyTo = variableSourceAssets.Where((elem) => elem.AssetId == saveData.AssetId).FirstOrDefault();
            if (toApplyTo == null)
            {
                Debug.LogWarning($"No VariableSourceAsset with AssetId {saveData.AssetId} was found to apply save data to.");
                return Task.CompletedTask;
            }

            foreach (VariableSaveData varSaveData in saveData.SavedVars)
            {
                IVarCodec forThisVar = validCodecs.FirstOrDefault(elem => elem.CanHandle(varSaveData));
                if (forThisVar == null)
                {
                    Debug.LogWarning($"No codec found for variable type: {varSaveData.GetType().Name}");
                    continue;
                }

                IVariable varEl = toApplyTo.GetVariable(varSaveData.ItemId);
                varEl ??= toApplyTo.GetVariable(varSaveData.VarName); // Fallback to searching by name

                if (varEl == null)
                {
                    Debug.LogWarning($"Variable {varSaveData.VarName} not found in flowchart {toApplyTo.name}.");
                    continue;
                }

                forThisVar.Decode(varEl, varSaveData);
            }

            return Task.CompletedTask;
        }

        public override Task Apply(SaveData saveData)
        {
            return Apply(saveData as VariableSourceAssetSaveData);
        }
    }
}