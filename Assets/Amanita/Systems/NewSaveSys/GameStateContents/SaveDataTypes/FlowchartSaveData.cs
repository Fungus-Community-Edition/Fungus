using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class FlowchartSaveData : SaveData
    {
        // When finding which flowchart this should be applied to, we search
        // by ID first. If not found, then we search by name.
        [SerializeField] protected string uniqueId = string.Empty;
        [SerializeField] protected string flowchartName = string.Empty;
        [SerializeField] protected List<VariableSaveData> savedVars = new();
        [SerializeField] protected List<BlockSaveData> savedBlocks = new();

        public virtual string UniqueId
        {
            get => uniqueId;
            set => uniqueId = value;
        }

        public virtual string FlowchartName
        {
            get => flowchartName;
            set => flowchartName = value;
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

        public virtual IList<BlockSaveData> SavedBlocks
        {
            get => savedBlocks;
            set
            {
                savedBlocks.Clear();
                savedBlocks.AddRange(value);
            }
        }

        public FlowchartSaveData()
        {
            // Default constructor for serialization
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
            else
            {
                Debug.LogWarning($"Variable '{varName}' not found in flowchart '{flowchartName}'.");
            }

            return result;
        }

    }

}