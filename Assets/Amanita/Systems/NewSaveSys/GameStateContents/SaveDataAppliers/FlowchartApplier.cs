using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Amanita.Utils;
using System;
using Amanita.VScripting;
using UnityEngine.SceneManagement;
using Amanita.VScripting.EventHandlers;

namespace Amanita.SaveSys
{
    [SaveSysDisplayName("Flowchart Applier (Amanita Default)")]
    public class FlowchartApplier : SaveDataApplier<FlowchartSaveData>
    {
        protected IList<Flowchart> allFlowcharts = new List<Flowchart>();

        protected virtual void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        protected void OnActiveSceneChanged(Scene _, Scene __)
        {
            // When the scene changes, the flowcharts in the previous scene may be destroyed,
            // so we clear the list to avoid holding onto invalid references.
            allFlowcharts.Clear();
        }

        protected virtual void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        protected virtual void OnDestroy()
        {
            allFlowcharts.Clear();
        }

        protected virtual void OnValidate()
        {
            RemoveFakeNullFlowcharts();
        }

        protected virtual void RemoveFakeNullFlowcharts()
        {
            if (allFlowcharts != null)
            {
                // Editor-time prune; Unity “fake null” evaluates true here
                for (int i = 0; i < allFlowcharts.Count; i++)
                {
                    if (allFlowcharts[i] == null)
                    {
                        allFlowcharts.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public override Task Apply(FlowchartSaveData saveData)
        {
            Flowchart flowchart = null;

            bool flowchartFound = false;
            string flowchartNotFoundMessage = $"Flowchart with ID {saveData.UniqueId} or name {saveData.FlowchartName} not found.";
            bool onMainThread = UnityThreadUtil.IsMainThread;
            void MainOperation()
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
            if (onMainThread)
            {
                MainOperation();
            }
            else
            {
                // Push to main thread to touch scene objects safely
                using (var countdown = new CountdownEvent(1))
                {
                    bool enqueued = false;
                    try
                    {
                        MainThreadDispatcher.Enqueue(() =>
                        {
                            MainOperation();
                            countdown.Signal();
                        });
                        enqueued = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    finally
                    {
                        if (!enqueued)
                        {
                            // ensure we don’t deadlock if enqueue fails
                            countdown.Signal();
                        }
                    }
                    countdown.Wait();
                }
            }

            return Task.CompletedTask;

            void ApplyStuff()
            {
                RemoveGameStartedEventHandlers();
                ApplyVarStates();
                ApplyBlockStates();
            }

            void RemoveGameStartedEventHandlers()
            {
                // Remember: Flowcharts should only get a chance to call their Start methods _after_
                // all the appliers (including this one) have done their thing.
                // Thus, this is a safe time to remove any GameStarted event handlers,
                // making sure they don’t get triggered prematurely.
                var gameStartedBlocks = flowchart.GetComponents<Block>()
                    .Where(blockEl => blockEl._EventHandler is GameStarted);
                foreach (var blockEl in gameStartedBlocks)
                {
                    blockEl._EventHandler = null;
                }
            }

            void ApplyVarStates()
            {
                foreach (VariableSaveData varSaveData in saveData.SavedVars)
                {
                    IVarCodec forThisVar = VarCodecRegistry.GetCodec(varSaveData);
                    if (forThisVar == null)
                    {
                        Debug.LogWarning($"No codec found for variable type: {varSaveData.GetType().Name}");
                        continue;
                    }

                    IVariable varEl = flowchart.GetVariableById(varSaveData.ItemId);
                    varEl ??= flowchart.GetVariable(varSaveData.VarName); // Fallback to searching by name

                    if (varEl == null)
                    {
                        Debug.LogWarning($"Variable {varSaveData.VarName} not found in flowchart {flowchart.name}.");
                        continue;
                    }

                    forThisVar.ApplyState(varEl, varSaveData);
                }
            }

            void ApplyBlockStates()
            {
                foreach (BlockSaveData blockSave in saveData.SavedBlocks)
                {
                    Block blockToApplyTo = FindTheRightBlock(flowchart, blockSave);

                    if (blockToApplyTo == null)
                    {
                        Debug.LogWarning($"Block {blockSave.BlockName} not found in flowchart {flowchart.name}.");
                        continue;
                    }

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

                static Block FindTheRightBlock(Flowchart fc, BlockSaveData blockSave)
                {
                    return fc.FindBlockByItemId(blockSave.ItemId) ?? fc.FindBlock(blockSave.BlockName);
                }
            }
        }

        protected virtual bool TryGetFlowchartFor(FlowchartSaveData saveData, out Flowchart flowchart)
        {
            // Prune stale “fake null” entries first
            PruneNulls(allFlowcharts);

            // If empty, (re)build the cache; include inactive flowcharts
            if (allFlowcharts.Count == 0)
            {
                allFlowcharts = FindObjectsByType<Flowchart>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

            }

            flowchart = FindFlowchartById(saveData.UniqueId) ?? FindFlowchartByName(saveData.FlowchartName);
            return flowchart != null;
        }

        protected virtual void PruneNulls(IList<Flowchart> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                }
            }
        }

        protected virtual Flowchart FindFlowchartById(string id)
        {
            return (from flowchart in allFlowcharts
                    where flowchart != null
                    where flowchart.UniqueId == id
                    select flowchart).FirstOrDefault();
        }

        protected virtual Flowchart FindFlowchartByName(string name)
        {
            return (from flowchart in allFlowcharts
                    where flowchart != null
                    where flowchart.name == name
                    select flowchart).FirstOrDefault();
        }

        public override Task Apply(SaveData saveData)
        {
            return Apply(saveData as FlowchartSaveData);
        }
    }
}