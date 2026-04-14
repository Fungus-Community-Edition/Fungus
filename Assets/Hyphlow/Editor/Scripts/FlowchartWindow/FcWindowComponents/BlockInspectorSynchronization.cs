using System;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Keeps FlowchartWindow selections in sync while delegating inspector ownership to BlockInspectorManager.
    /// </summary>
    public sealed class BlockInspectorSynchronization
    {
        private readonly Func<Flowchart> flowchartProvider;
        private readonly Action<Block> blockSelector;

        public BlockInspectorSynchronization(
            Func<Flowchart> flowchartProvider,
            Action<Block> blockSelector = null)
        {
            this.flowchartProvider = flowchartProvider ?? throw new ArgumentNullException(nameof(flowchartProvider));
            this.blockSelector = blockSelector;
        }

        public Block LastShownBlock { get; private set; }

        public void ResetLastShownBlock()
        {
            Flowchart flowchart = Flowchart;
            LastShownBlock = flowchart != null ? 
                flowchart.SelectedBlock : 
                null;
        }

        public void HandleBlockCreated(Block block)
        {
            if (block == null)
            {
                HandleEmptySpaceClicked();
                return;
            }

            if (blockSelector != null)
            {
                SelectBlock(block);
            }
            else
            {
                ShowThroughManager(block);
            }
        }

        public void HandleBlockClicked(Block block)
        {
            if (block == null)
            {
                HandleEmptySpaceClicked();
                return;
            }

            SelectBlock(block);
            ShowThroughManager(block);
        }

        public void HandleEmptySpaceClicked()
        {
            Flowchart flowchart = Flowchart;
            LastShownBlock = null;

            if (flowchart != null)
            {
                flowchart.ClearSelectedBlocks();
                flowchart.ClearSelectedCommands();
            }

            if (flowchart != null && Selection.activeGameObject != flowchart.gameObject)
            {
                Selection.activeGameObject = flowchart.gameObject;
            }
        }

        public void SyncInspectorWithSelectionIfNeeded()
        {
            Flowchart flowchart = Flowchart;
            if (flowchart == null)
            {
                return;
            }

            GameObject selectedGameObject = Selection.activeGameObject;
            bool flowchartIsSelected = selectedGameObject != null &&
                selectedGameObject.GetComponent<Flowchart>() != null;

            bool changedBlockSelection = flowchart.SelectedBlock != LastShownBlock;
            bool alreadyShowingSelectedBlock = BlockInspectorManager.LastShownBlock == flowchart.SelectedBlock;

            if (!flowchartIsSelected || !changedBlockSelection || alreadyShowingSelectedBlock)
            {
                return;
            }

            LastShownBlock = flowchart.SelectedBlock;

            if (LastShownBlock != null)
            {
                BlockInspectorManager.Show(LastShownBlock);
            }
            else
            {
                BlockInspectorManager.Clear();
            }
        }

        private void SelectBlock(Block block)
        {
            if (blockSelector != null && block != null)
            {
                blockSelector(block);
            }
        }

        private void ShowThroughManager(Block block)
        {
            LastShownBlock = block;

            if (block == null)
            {
                BlockInspectorManager.Clear();
            }
            else
            {
                BlockInspectorManager.Show(block);
            }
        }

        private Flowchart Flowchart => flowchartProvider != null ? flowchartProvider() : null;
    }
}