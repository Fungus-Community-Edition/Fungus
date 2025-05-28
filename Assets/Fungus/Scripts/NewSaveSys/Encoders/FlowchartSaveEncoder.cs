using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "FlowchartSaveEncoder",
        menuName = "Amanita/SaveSys/FlowchartSaveEncoder")]
    public class FlowchartSaveEncoder : SaveEncoder<Flowchart, FlowchartSaveData>
    {
        public new Flowchart ToMakeFrom
        {
            get { return (Flowchart)base.ToMakeFrom; }
            set { base.ToMakeFrom = value; }
        }

        protected virtual void OnEnable()
        {
            if (blockEncoder == null)
            {
                blockEncoder = CreateInstance<BlockSaveEncoder>();
            }
        }

        protected BlockSaveEncoder blockEncoder;

        public override FlowchartSaveData EncodeToSave(Flowchart toCreateFrom)
        {
            IList<VariableSaveData> varSaves = SaveVars(toCreateFrom);
            IList<BlockSaveData> blockSaves = blockEncoder.EncodeToMultiSave(toCreateFrom);
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

                VariableSaveData varSave = forThisVar.EncodeToSave(varEl);
                if (varSave == null)
                {
                    Debug.LogError($"Failed to encode variable: {varEl.name}");
                    continue;
                }

                savedVars.Add(varSave);
            }

            return savedVars;
        }

        public override SaveDataUnit EncodeToUnit()
        {
            return EncodeToUnit(ToMakeFrom);
        }

        public override SaveDataUnit EncodeToUnit(Flowchart from)
        {
            FlowchartSaveData saveData = EncodeToSave(from);
            SaveDataUnit result = saveData.Serialized();
            return result;
        }

    }

}