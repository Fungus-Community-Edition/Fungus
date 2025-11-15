using UnityEngine;
using System.Collections.Generic;

namespace Amanita.SaveSys
{
    public class VariableSourceAssetSaveData : SaveData
    {
        [SerializeField] protected string uniqueId = string.Empty;
        [SerializeField] protected List<VariableSaveData> savedVars = new List<VariableSaveData>();

        public virtual string UniqueId
        {
            get => uniqueId;
            set => uniqueId = value;
        }

        public virtual IList<VariableSaveData> SavedVars
        {
            get => savedVars;
            set
            {
                savedVars.Clear();
                savedVars.AddRange(value);
            }
        }

        public virtual T GetVarValue<T>(string varName)
        {
            T result = default;
            VariableSaveData foundVar = savedVars.Find(v => v.VarName == varName);

            if (foundVar != null)
            {
                IVarCodec codec = VarCodecRegistry.GetCodec(foundVar.VarTypeName);
                if (codec != null)
                {
                    result = codec.DecodeTo<T>(foundVar.Value);
                }
                else
                {
                    Debug.LogError($"Codec for variable type '{foundVar.VarTypeName}' not found.");
                }
            }

            return result;
        }
    }
}