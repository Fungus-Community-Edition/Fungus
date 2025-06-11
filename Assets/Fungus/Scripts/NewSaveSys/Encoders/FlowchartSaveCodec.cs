using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "FlowchartCodec",
        menuName = "Amanita/SaveSys/Codecs/FlowchartSaveCodec")]
    public class FlowchartSaveCodec : SaveCodec<Flowchart, FlowchartSaveData>, IMainSaveCodec
    {
        public new Flowchart ToMakeFrom
        {
            get { return base.ToMakeFrom; }
            set { base.ToMakeFrom = value; }
        }

        protected virtual void OnEnable()
        {
            if (blockCodec == null)
            {
                blockCodec = CreateInstance<BlockSaveCodec>();
            }
        }

        protected BlockSaveCodec blockCodec;

        public override FlowchartSaveData EncodeToSave(Flowchart toCreateFrom)
        {
            IList<VariableSaveData> varSaves = SaveVars(toCreateFrom);
            IList<BlockSaveData> blockSaves = blockCodec.EncodeToMultiSave(toCreateFrom);
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
            IList<VariableSaveData> result = new List<VariableSaveData>();

            if (toCreateFrom.SaveVariables)
            {
                foreach (Variable varEl in toCreateFrom.Variables)
                {
                    IVarCodec forThisVar = CodecRegistry.GetCodec(varEl);
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

                    result.Add(varSave);
                }
            }

            return result;
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

        public override IList<SaveDataUnit> FindAndEncodeAll()
        {
            IList<Flowchart> allFlowcharts = FindObjectsByType<Flowchart>(FindObjectsSortMode.None);

            allFlowcharts = (from elem in allFlowcharts
                             where elem.SaveVariables == true
                             select elem).ToList();

            IList<SaveDataUnit> results = new List<SaveDataUnit>();

            for (int i = 0; i < allFlowcharts.Count; i++)
            {
                Flowchart currentFlowchart = allFlowcharts[i];
                SaveDataUnit newUnit = EncodeToUnit(currentFlowchart);
                results.Add(newUnit);
            }

            return results;
        }
    
        public override SaveData DecodeFrom(SaveDataUnit unit)
        {
            if (unit == null)
            {
                Debug.LogError("Cannot decode from a null SaveDataUnit.");
                return null;
            }
            FlowchartSaveData saveData = JsonUtility.FromJson<FlowchartSaveData>(unit.Content);
            if (saveData == null)
            {
                Debug.LogError($"Failed to decode {unit.DataTypeName} to FlowchartSaveData.");
                return null;
            }
            return saveData;
        }

        public override bool CanHandle(string typeName)
        {
            return typeName == nameof(Flowchart) || typeName == nameof(FlowchartSaveData);
        }

    }

}