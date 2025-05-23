using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class FlowchartSaveEncoder : SaveEncoder<FlowchartSaveData, Flowchart>
    {
        public override FlowchartSaveData Encode(Flowchart toCreateFrom)
        {
            IList<VariableSaveData> varSaves = SaveVars(toCreateFrom);
            IList<BlockSaveData> blockSaves = SaveBlocks(toCreateFrom);
            // TODO: Save the state of certain commands (such as Conversation)


            FlowchartSaveData saveData = new()
            {
                UniqueId = toCreateFrom.UniqueId,
                FlowchartName = toCreateFrom.name,
                SavedVars = varSaves,
                SavedBlocks = blockSaves,
            };

            return saveData;
        }


        protected virtual IList<VariableSaveData> SaveVars(Flowchart toCreateFrom)
        {
            IList<VariableSaveData> savedVars = new List<VariableSaveData>();
            foreach (Variable varEl in toCreateFrom.Variables)
            {
                IVarEncoder forThisVar = EncoderRegistry.GetEncoder(varEl);
                if (forThisVar == null)
                {
                    Debug.LogWarning($"No serializer found for variable type: {varEl.GetType().Name}");
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

            return savedVars;
        }

        protected virtual IList<BlockSaveData> SaveBlocks(Flowchart toCreateFrom)
        {
            IList<BlockSaveData> savedBlocks = new List<BlockSaveData>();
            foreach (Block block in toCreateFrom.GetExecutingBlocks())
            {
                BlockSaveData blockSave = new(block);
                savedBlocks.Add(blockSave);
            }
            return savedBlocks;
        }
    }
}