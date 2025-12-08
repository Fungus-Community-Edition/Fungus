using Amanita.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Amanita.VScripting;
using Amanita.FSExt;
using FullSerializer;

namespace Amanita.SaveSys
{
    [SaveSysDisplayName("Flowchart Codec (Amanita Default)")]
    public class FlowchartSaveCodec : SaveCodec<Flowchart, FlowchartSaveData>, IMainSaveCodec, IMainSaveDataProducer
    {
        public virtual void PreInstallInit()
        {
            // Nothing to init for now
        }

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
            UnityThreadUtil.RunOnMainThread(EncodingProcess);
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
                foreach (IVariable varEl in variables)
                {
                    IVarCodec forThisVar = VarCodecRegistry.GetCodec(varEl);
                    if (forThisVar == null)
                    {
                        Debug.LogWarning($"No codec found for variable type: {varEl.GetType().Name}");
                        continue;
                    }

                    VariableSaveData varSave = forThisVar.EncodeToSave(varEl);
                    if (varSave == null)
                    {
                        Debug.LogError($"Failed to encode variable: {varEl.Key}");
                        continue;
                    }

                    result.Add(varSave);
                }
            }

            return result;
        }

        public override bool CanHandle(string typeName)
        {
            return typeName == nameof(Flowchart) || typeName == nameof(FlowchartSaveData);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
        }

        public IList<SaveData> FindAndCreateAll(System.Action<IList<SaveData>> onComplete = null)
        {
            IList<SaveData> results = new List<SaveData>();
            using (var countdown = new CountdownEvent(1))
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    IList<Flowchart> allFlowcharts = FindObjectsByType<Flowchart>(FindObjectsSortMode.None);

                    IList<Flowchart> flowchartsToSave = (from elem in allFlowcharts
                                                         where elem.IncludeInSaves == true
                                                         select elem).ToList();

                    for (int i = 0; i < flowchartsToSave.Count; i++)
                    {
                        Flowchart toSave = flowchartsToSave[i];
                        var data = EncodeToSave(toSave);
                        if (data != null)
                        {
                            results.Add(data);
                        }
                    }

                    countdown.Signal();
                });
                countdown.Wait();
            }

            onComplete?.Invoke(results);
            return results;
        }

        public override FlowchartSaveData Decode(string rawText)
        {
            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                FlowchartSaveData result = serializer.FromJson<FlowchartSaveData>(rawText);
                return result;
            }
        }
    }
}