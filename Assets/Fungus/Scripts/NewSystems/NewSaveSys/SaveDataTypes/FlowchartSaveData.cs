using Fungus;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class FlowchartSaveData : SaveData
    {
        // When finding which flowchart this should be applied to, we search
        // by ID first. If not found, then we search by name.
        [SerializeField] protected string flowchartID = string.Empty;
        [SerializeField] protected string flowchartName = string.Empty;
        [SerializeField] protected List<VariableSaveData> savedVars = new();
        [SerializeField] protected List<BlockSaveData> savedBlocks = new();

        public FlowchartSaveData(Flowchart toCreateFrom)
        {
            SaveIdentifier identifier = toCreateFrom.GetComponent<SaveIdentifier>();
            flowchartID = identifier.UniqueID;
            flowchartName = toCreateFrom.name;
            SaveVars(toCreateFrom);
            SaveBlocks(toCreateFrom);

            // TODO: Save the state of certain commands (such as Conversation)

        }

        protected virtual void SaveVars(Flowchart toCreateFrom)
        {
            foreach (Variable varEl in toCreateFrom.Variables)
            {
                IVarEncoder forThisVar = EncoderRegistry.GetEncoder(varEl);
                if (forThisVar == null)
                {
                    Debug.LogError($"No serializer found for variable type: {varEl.GetType().Name}");
                    continue;
                }

                VariableSaveData varSave = forThisVar.Encode(varEl);
                if (varSave == null)
                {
                    Debug.LogError($"Failed to encode variable: {varEl.name}");
                    continue;
                }

                savedVars.Add(varSave);
            }
        }

        protected virtual void SaveBlocks(Flowchart toCreateFrom)
        {
            foreach (Block block in toCreateFrom.GetExecutingBlocks())
            {
                BlockSaveData blockSave = new(block);
                savedBlocks.Add(blockSave);
            }
        }

        public override SerializedSaveData Serialized()
        {
            string json = JsonUtility.ToJson(this, true);
            string typeName = GetType().Name;
            SerializedSaveData newItem = new(typeName, json);
            return newItem;
        }

    }

}