using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Amanita.Utils;
using System;
using Amanita.VScripting;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "FlowchartApplier",
        menuName = "Amanita/SaveSys/FlowchartApplier",
        order = 0)]
    public class FlowchartApplier : SaveDataApplier<FlowchartSaveData>
    {
        [SerializeField] protected ScriptableObject[] varCodecs = Array.Empty<ScriptableObject>();

        public virtual void RegisterVarCodec(IVarCodec codec)
        {
            if (codec == null)
            {
                Debug.LogWarning("Cannot register a null codec.");
                return;
            }
            if (!validVarCodecs.Contains(codec))
            {
                validVarCodecs.Add(codec);
            }
        }

        protected virtual void OnEnable()
        {
            RefreshValidCodecs();
        }

        protected virtual void RefreshValidCodecs()
        {
            validVarCodecs.Clear();
            foreach (var codecObj in varCodecs)
            {
                if (codecObj is IVarCodec codec)
                {
                    validVarCodecs.Add(codec);
                }
                else if (codecObj != null)
                {
                    Debug.LogWarning($"Object {codecObj.name} is not an IVarCodec.");
                }
            }
        }
        protected IList<IVarCodec> validVarCodecs = new List<IVarCodec>();

        protected virtual void OnValidate()
        {
            if (allFlowcharts != null)
            {
                allFlowcharts = allFlowcharts.Where(fc => fc != null).ToList();
            }
            RefreshValidCodecs();
        }

        protected IList<Flowchart> allFlowcharts = new List<Flowchart>();

        public override Task Apply(FlowchartSaveData saveData)
        {
            // Applying the states of vars and Blocks might require tampering with things
            // that aren't thread-safe. Also, funcs like FindObjectsByType only work on the main thread.
            Flowchart flowchart = null;

            bool flowchartFound = false;
            string flowchartNotFoundMessage = $"Flowchart with ID {saveData.UniqueId} or name {saveData.FlowchartName} not found.";
            bool onMainThread = UnityThreadUtil.IsMainThread;
            if (onMainThread)
            {
                flowchartFound = TryGetFlowchartFor(saveData, out flowchart);
                if (!flowchartFound)
                {
                    Debug.LogWarning(flowchartNotFoundMessage);
                }
                else
                {
                    ApplyStuff();
                }
            }

            void ApplyStuff()
            {
                ApplyVarStates();
                ApplyBlockStates();
            }
            void ApplyVarStates()
            {
                foreach (VariableSaveData varSaveData in saveData.SavedVars)
                {
                    IVarCodec forThisVar = validVarCodecs.FirstOrDefault(c => c.CanHandle(varSaveData));
                    if (forThisVar == null)
                    {
                        Debug.LogWarning($"No codec found for variable type: {varSaveData.GetType().Name}");
                        continue;
                    }

                    IVariable varEl = flowchart.GetVariableById(varSaveData.ItemID);
                    varEl ??= flowchart.GetVariable(varSaveData.VarName); // Fallback to searching by name

                    bool stillGotNothing = varEl == null;
                    if (stillGotNothing)
                    {
                        Debug.LogWarning($"Variable {varSaveData.VarName} not found in flowchart {flowchart.name}.");
                        continue;
                    }

                    forThisVar.Decode(varEl, varSaveData);
                }
            }
            void ApplyBlockStates()
            {
                foreach (BlockSaveData blockSave in saveData.SavedBlocks)
                {
                    Block blockToApplyTo = FindTheRightBlock(flowchart, blockSave);
                    Block FindTheRightBlock(Flowchart flowchart, BlockSaveData blockSave)
                    {
                        // Find the current block first by its item id, then by its name
                        Block blockToApplyTo = flowchart.FindBlockByItemId(blockSave.ItemId);
                        if (blockToApplyTo == null)
                        {
                            blockToApplyTo = flowchart.FindBlock(blockSave.BlockName);
                        }

                        return blockToApplyTo;
                    }

                    bool stllGotNothing = blockToApplyTo == null;
                    if (stllGotNothing)
                    {
                        Debug.LogWarning($"Block {blockSave.BlockName} not found in flowchart {flowchart.name}.");
                        continue;
                    }

                    // We assume that the Block was indeed executing
                    // at this point.
                    bool blockWasExecuting = blockSave.ActiveCommandIndex != -1;
                    if (blockWasExecuting)
                    {
                        Command commandToApplyTo = blockToApplyTo.FindCommandByID(blockSave.ActiveCommandId);

                        if (commandToApplyTo == null)
                        {
                            commandToApplyTo = blockToApplyTo.FindCommandByIndex(blockSave.ActiveCommandIndex);
                        }

                        bool stillNothing = commandToApplyTo == null;
                        if (stillNothing)
                        {
                            Debug.LogWarning($"Command {blockSave.ActiveCommandId} not found in block {blockToApplyTo.BlockName}.");
                            continue;
                        }

                        flowchart.StopBlock(blockSave.BlockName);
                        flowchart.ExecuteBlock(blockToApplyTo, blockSave.ActiveCommandIndex);

                    }
                }


            }

            if (!onMainThread)
            {
                PushTheWorkToTheMainThread();
                void PushTheWorkToTheMainThread()
                {
                    using (var countdown = new CountdownEvent(1))
                    {
                        bool lambdaEnqueued = false;
                        Exception threadException = null;
                        try
                        {
                            Debug.Log("Right before the enqueue");
                            MainThreadDispatcher.Enqueue(() =>
                            {
                                Debug.Log($"At start of pushing work to main thread");
                                flowchartFound = TryGetFlowchartFor(saveData, out flowchart);
                                if (!flowchartFound)
                                {
                                    Debug.LogWarning(flowchartNotFoundMessage);
                                }
                                else
                                {
                                    ApplyStuff();
                                }
                                Debug.Log($"Right before countdown.Signal when work on main thread is done");
                                countdown.Signal();
                            });
                            lambdaEnqueued = true;
                            Debug.Log("Right after the enqueue");
                        }
                        catch (Exception ex)
                        {
                            threadException = ex;
                        }
                        finally
                        {
                            if (!lambdaEnqueued)
                            {
                                countdown.Signal(); 
                                // ^Calling this after a successful enqueue can result in an ObjectDisposedException
                            }
                        }
                        countdown.Wait();
                    }
                    
                }
            }

            return Task.CompletedTask;
        }

        protected virtual bool TryGetFlowchartFor(FlowchartSaveData saveData, out Flowchart flowchart)
        {
            bool onMainThread = UnityThreadUtil.IsMainThread;
            if (allFlowcharts.Count == 0 || allFlowcharts.Contains(null))
            {
                allFlowcharts = FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
            }

            flowchart = FindFlowchartReferredToBy(saveData);
            Flowchart FindFlowchartReferredToBy(FlowchartSaveData saveData)
            {
                Flowchart flowchart = FindFlowchartById(saveData.UniqueId);
                if (flowchart == null)
                {
                    flowchart = FindFlowchartByName(saveData.FlowchartName);
                }

                return flowchart;
            }

            return flowchart != null;
        }

        protected virtual Flowchart FindFlowchartById(string id)
        {
            Flowchart result = (from flowchart in allFlowcharts
                                where flowchart.UniqueId == id
                                select flowchart).FirstOrDefault();
            return result;
        }

        protected virtual Flowchart FindFlowchartByName(string name)
        {
            Flowchart result = (from flowchart in allFlowcharts
                                where flowchart != null
                                where flowchart.name == name
                                select flowchart).FirstOrDefault();
            return result;
        }

        public override Task Apply(SaveData saveData)
        {
            return Apply(saveData as FlowchartSaveData);
        }
    }
}