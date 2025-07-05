using Amanita.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            if (!toCreateFrom.IncludeInSaves)
            {
                Debug.LogWarning($"Flowchart {toCreateFrom.name} is set to not be included in saves. Thus, it shall not be encoded.");
                return null;
            }

            // We want this whole func to run on the main thread,
            // since it might involve Unity API calls that are not thread-safe.
            IList<VariableSaveData> varSaves = null;
            IList<BlockSaveData> blockSaves = null;
            FlowchartSaveData saveData = null;
            void EncodingProcess()
            {
                varSaves = SaveVars(toCreateFrom);
                blockSaves = blockCodec.EncodeToMultiSave(toCreateFrom);
                // TODO: Save the state of certain commands (such as Conversation)

                saveData = new()
                {
                    UniqueId = toCreateFrom.UniqueId,
                    FlowchartName = toCreateFrom.name,
                    SavedVars = varSaves,
                    SavedBlocks = blockSaves,
                };
            }
            if (UnityThreadUtil.IsMainThread)
            {
                EncodingProcess();
            }
            else
            {
                using (var countdown = new CountdownEvent(1))
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        if (toCreateFrom == null)
                        {
                            Debug.LogError("Cannot encode a null Flowchart.");
                        }
                        else
                        {
                            Debug.Log("Right before encoding process.");
                            EncodingProcess();
                        }

                        countdown.Signal(); // Signal that we're done
                    });
                    countdown.Wait(); // Wait for the main thread to finish
                }
            }

            return saveData;
        }

        protected virtual IList<VariableSaveData> SaveVars(Flowchart toCreateFrom)
        {
            IList<VariableSaveData> result = new List<VariableSaveData>();

            var variables = toCreateFrom.Variables;
            int count = variables.Count;
            if (count == 0 || !toCreateFrom.IncludeInSaves)
            {
                // Do nothing and just return an empty list later in this func
            }
            else 
            {
                foreach (Variable varEl in variables)
                {
                    IVarCodec forThisVar = CodecRegistry.GetCodec(varEl);
                    if (forThisVar == null)
                    {
                        Debug.LogWarning($"No serializer found for variable type: {varEl.GetType().Name}");
                        continue;
                    }

                    var varSave = forThisVar.EncodeToSave(varEl);
                    if (varSave == null)
                    {
                        Debug.LogError($"Failed to encode variable: {varEl.name}");
                    }
                    else
                    {
                        result.Add(varSave);
                    }

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

        public virtual IList<SaveDataUnit> FindAndEncodeAll(System.Action<IList<SaveDataUnit>> onComplete = null)
        {
            IList<SaveDataUnit> results = new List<SaveDataUnit>();
            using (var countdown = new CountdownEvent(1))
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    IList<Flowchart> allFlowcharts;
                    
#if UNITY_6000_0_OR_NEWER
                    allFlowcharts = FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
#else
                    allFlowcharts = FindObjectsOfType<Flowchart>();
#endif

                    IList<Flowchart> flowchartsToSave = (from elem in allFlowcharts
                                                            where elem.IncludeInSaves == true
                                                            select elem).ToList();

                    for (int i = 0; i < flowchartsToSave.Count; i++)
                    {
                        Flowchart toSave = flowchartsToSave[i];
                        SaveDataUnit newUnit = EncodeToUnit(toSave);
                        results.Add(newUnit);
                    }

                    countdown.Signal(); // Signal that we're done
                });
                countdown.Wait(); // Wait for the main thread to finish
            }
                
            onComplete?.Invoke(results);
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