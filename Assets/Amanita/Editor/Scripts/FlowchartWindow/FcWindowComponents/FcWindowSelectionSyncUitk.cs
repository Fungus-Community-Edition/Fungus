using System;
using UnityEditor;
using UnityEngine;
using Amanita.VScripting;

namespace Amanita.VScripting.EditorUtils
{
    public sealed class FcWindowSelectionSyncUitk : IFlowchartWindowModule,
        IEmptySpaceClickResponder, IFlowchartChangeResponder, IBlockSelectionResponder
    {
        private readonly FlowchartContext flowchartContext;
        private FlowchartWindowUitk owner;
        private Flowchart trackedFlowchart;
        private Block lastShownBlock;
        private bool skipNextEditorUpdate;
        private int prevVarCount;
        private bool isDisposed;

        public FcWindowSelectionSyncUitk(FlowchartContext context)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Initialize(FlowchartWindowUitk window)
        {
            owner = window ?? throw new ArgumentNullException(nameof(window));

            SubscribeToFlowchart(flowchartContext.Flowchart);
            BlockSignals.BlockCreated += OnBlockCreated;
            BlockSignals.BlockClicked += OnBlockClicked;
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            BlockSignals.BlockCreated -= OnBlockCreated;
            BlockSignals.BlockClicked -= OnBlockClicked;
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.update -= OnEditorUpdate;
            SubscribeToFlowchart(null);
            owner = null;
            lastShownBlock = null;
        }

        public void OnBlockSelected(Block block)
        {
            if (!isDisposed)
            {
                ShowInspectorForBlock(block);
            }
        }

        public void OnEmptySpaceClicked(Vector2 pos)
        {
            if (!isDisposed)
            {
                ShowInspectorForBlock(null);
            }
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            if (isDisposed)
            {
                return;
            }

            skipNextEditorUpdate = true;
            SubscribeToFlowchart(next);
            lastShownBlock = next != null ? next.SelectedBlock : null;
            ShowInspectorForBlock(lastShownBlock);
        }

        private void OnEditorUpdate()
        {
            if (isDisposed || owner == null)
            {
                return;
            }

            Flowchart flowchart = Flowchart;
            if (flowchart == null)
            {
                owner.UpdateBlockCollection();
                owner.Repaint();
                return;
            }

            if (skipNextEditorUpdate)
            {
                skipNextEditorUpdate = false;
                return;
            }

            if (AnyNullBlocks())
            {
                owner.UpdateBlockCollection();
                owner.Repaint();
                return;
            }

            bool repaintRequested = false;

            if (flowchart.VariableCount != prevVarCount)
            {
                prevVarCount = flowchart.VariableCount;
                repaintRequested = true;
            }

            if (flowchart.SelectedCommandsStale)
            {
                flowchart.SelectedCommandsStale = false;
                repaintRequested = true;
            }

            if (CommandEditor.SelectedCommandDataStale)
            {
                CommandEditor.SelectedCommandDataStale = false;
                repaintRequested = true;
            }

            if (BlockEditor.SelectedBlockDataStale)
            {
                BlockEditor.SelectedBlockDataStale = false;
                repaintRequested = true;
            }

            if (FlowchartEditor.FlowchartDataStale)
            {
                FlowchartEditor.FlowchartDataStale = false;
                repaintRequested = true;
            }

            if (repaintRequested)
            {
                owner.Repaint();
            }
        }

        private void OnBlockCreated(Block block)
        {
            if (!isDisposed)
            {
                ShowInspectorForBlock(block);
            }
        }

        private void OnBlockClicked(Block block, Event _)
        {
            if (!isDisposed)
            {
                ShowInspectorForBlock(block);
            }
        }

        private void OnSelectionChanged()
        {
            if (isDisposed)
            {
                return;
            }

            Flowchart flowchart = Flowchart;
            if (flowchart == null)
            {
                return;
            }

            GameObject selectedGo = Selection.activeGameObject;
            if (selectedGo == null || !selectedGo.TryGetComponent(out Flowchart selectedFlowchart))
            {
                return;
            }

            if (!ReferenceEquals(selectedFlowchart, flowchart))
            {
                return;
            }

            Block selectedBlock = flowchart.SelectedBlock;
            if (selectedBlock == null || ReferenceEquals(selectedBlock, lastShownBlock))
            {
                return;
            }

            bool alreadyShowing = BlockInspector != null && BlockInspector.block == selectedBlock;
            if (alreadyShowing)
            {
                return;
            }

            ShowInspectorForBlock(selectedBlock);
        }

        private void ShowInspectorForBlock(Block block)
        {
            Flowchart flowchart = Flowchart;
            if (flowchart == null)
            {
                return;
            }

            if (block == null)
            {
                if (BlockInspector != null)
                {
                    BlockInspector.block = null;
                }

                if (Selection.activeObject == BlockInspector)
                {
                    Selection.activeObject = flowchart.gameObject;
                }

                lastShownBlock = null;
                return;
            }

            bool inspectorIsActive = BlockInspector != null && Selection.activeObject == BlockInspector;
            bool inspectorAlreadyShowing = BlockInspector != null && BlockInspector.block == block;

            if (inspectorIsActive && inspectorAlreadyShowing)
            {
                return;
            }

            ShowBlockInspector(block);
            lastShownBlock = block;
        }

        private void ShowBlockInspector(Block block)
        {
            Flowchart flowchart = Flowchart;
            if (flowchart == null || block == null)
            {
                return;
            }

            bool alreadyShowingThatBlock = BlockInspector != null &&
                BlockInspector.block == block &&
                Selection.activeObject == BlockInspector;

            if (alreadyShowingThatBlock)
            {
                return;
            }

            EnsureBlockInspectorExists();

            if (BlockInspector == null)
            {
                return;
            }

            bool wasAlreadyShowingThisBlock = BlockInspector.block == block;
            if (!wasAlreadyShowingThisBlock)
            {
                flowchart.ClearSelectedCommands();
            }

            BlockInspector.block = block;

            if (block.ActiveCommand != null)
            {
                flowchart.AddSelectedCommand(block.ActiveCommand);
            }

            if (Selection.activeObject != BlockInspector)
            {
                Selection.activeObject = BlockInspector;
            }
        }

        private void EnsureBlockInspectorExists()
        {
            if (BlockInspector != null)
            {
                return;
            }

            BlockInspector inspector = ScriptableObject.CreateInstance<BlockInspector>();
            inspector.hideFlags = HideFlags.DontSave;
            EditorUtility.SetDirty(inspector);
            BlockInspector = inspector;
        }

        private void SubscribeToFlowchart(Flowchart next)
        {
            if (ReferenceEquals(trackedFlowchart, next))
            {
                return;
            }

            ToggleSubsOnTrackedFlowchart(false);
            trackedFlowchart = next;
            prevVarCount = trackedFlowchart != null ? 
                trackedFlowchart.VariableCount : 
                0;

            ToggleSubsOnTrackedFlowchart(true);

        }

        private void ToggleSubsOnTrackedFlowchart(bool on)
        {
            if (trackedFlowchart == null)
            {
                return;
            }

            if (on)
            {
                trackedFlowchart.VariableAdded += OnVariableChanged;
                trackedFlowchart.VariableRemoved += OnVariableChanged;
            }
            else
            {
                trackedFlowchart.VariableAdded -= OnVariableChanged;
                trackedFlowchart.VariableRemoved -= OnVariableChanged;
            }
        }

        private void OnVariableChanged(IVariable _)
        {
            if (trackedFlowchart == null || owner == null)
            {
                return;
            }

            prevVarCount = trackedFlowchart.VariableCount;
            owner.Repaint();
        }

        private bool AnyNullBlocks()
        {
            var blocks = flowchartContext.Document?.AllBlocks;
            if (blocks == null)
            {
                return false;
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        private Flowchart Flowchart => flowchartContext.Flowchart;

        private static BlockInspector BlockInspector
        {
            get => FlowchartWindow.blockInspector;
            set => FlowchartWindow.blockInspector = value;
        }
    }
}