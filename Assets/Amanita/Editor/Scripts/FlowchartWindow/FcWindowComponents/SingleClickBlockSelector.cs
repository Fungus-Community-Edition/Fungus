using System;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles single-click-driven block selection and empty space deselection.
    /// </summary>
    public sealed class SingleClickBlockSelector : IFlowchartWindowModule,
        IEmptySpaceClickResponder, IBlockClickResponder, IBlockCreatedResponder
    {
        public SingleClickBlockSelector(FlowchartContext context)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        private readonly FlowchartContext flowchartContext;
        
        public void Initialize(FlowchartWindowUitk window)
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
                Flowchart.ClearSelectedBlocks();
                Flowchart.ClearSelectedCommands();
                return;
            }

            if (block.IsSelected)
            {
                return;
            }

            Flowchart.ClearSelectedCommands();
            Flowchart.SelectedBlock = block;
        }

        private Flowchart Flowchart => flowchartContext.Flowchart;

        public void OnBlockCreated(Block block)
        {
            if (!isDisposed)
            {
                SetFlowchartAsSelecting(block);
            }
        }

        public void OnEmptySpaceClicked(Vector2 position)
        {
            if (isDisposed)
            {
                return;
            }

            SetFlowchartAsSelecting(null);

            if (Flowchart != null && Selection.activeGameObject != Flowchart.gameObject)
            {
                Selection.activeGameObject = Flowchart.gameObject;
            }
        }

    }
}