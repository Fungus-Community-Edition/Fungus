using System;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Handles single-click-driven block selection and empty space deselection.
    /// </summary>
    public sealed class SingleSelectionHandler : IFlowchartWindowModule, 
        IEmptySpaceLeftClickResponder, IBlockClickResponder, IBlockCreatedResponder
    {
        public int Priority { get; set; } = 0;
        public SingleSelectionHandler(FlowchartContext context)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        private readonly FlowchartContext flowchartContext;
        
        public void Initialize(FlowchartWindow window)
        {
            isDisposed = false;
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }
        }

        private bool isDisposed;

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
        }

        public void OnBlockClicked(Block block, Event _)
        {
            if (!isDisposed)
            {
                if (_.shift || _.control || _.command)
                {
                    return; // Let multi-selection handler deal with it.
                }
                SetFlowchartAsSelecting(block);
            }
        }

        private void SetFlowchartAsSelecting(Block block)
        {
            if (Flowchart == null)
            {
                return;
            }

            bool validBlock = block != null; // We assume that the block belongs to the flowchart.
            if (!validBlock) // Probably empty space clicked.
            {
                Flowchart.DeselectAll();
                return;
            }

            if (!block.IsSelected) // Single-clicking one block should deselect all other blocks and commands.
            {
                Flowchart.ClearSelectedCommands();
                Flowchart.ClearSelectedBlocks();
            }
            

            Flowchart.SelectedBlock = block;
            Flowchart.AddToSelection(block);
        }

        private Flowchart Flowchart => flowchartContext.Flowchart;

        public void OnBlockCreated(Block block)
        {
            if (!isDisposed)
            {
                SetFlowchartAsSelecting(block);
            }
        }

        public void OnEmptySpaceLeftClicked(PointerEventInfo info)
        {
            if (isDisposed || Flowchart == null)
            {
                return;
            }

            SetFlowchartAsSelecting(null);

            if (Selection.activeGameObject != Flowchart.gameObject)
            {
                Selection.activeGameObject = Flowchart.gameObject;
            }
        }

    }
}