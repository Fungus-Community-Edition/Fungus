using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    public sealed class BlockMovementHandler : IFlowchartWindowModule, ILeftMouseDragResponder,
        ILeftMouseUpResponder
    {
        public int Priority { get; set; } = 0;
        public BlockMovementHandler(FlowchartContext context)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        private readonly FlowchartContext flowchartContext;

        public void Initialize(FlowchartWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            isDisposed = false;
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

        public void OnLeftMouseDragged(PointerEventInfo info, Event evt)
        {
            if (isDisposed || evt == null || evt.alt)
            {
                return;
            }

            if (Flowchart == null || Interaction.RootBlockToDrag == null)
            {
                return;
            }

            // The SelectionState returns a defensive copy of the blocks. Thus for perf's sake, we'll cache
            // one here and pass it to the methods that need it.
            var blocks = SelectedBlocks;

            HandleUndoStack(blocks);
            Interaction.BlockDragOngoing = true;
            ApplyTheMovement(ref info, blocks);
            Interaction.HasDraggedSelected = true;
        }

        private Flowchart Flowchart => flowchartContext.Flowchart;
        private InteractionState Interaction => flowchartContext.Interaction;
        private IList<Block> SelectedBlocks => flowchartContext.Selection.Blocks;

        private void HandleUndoStack(IList<Block> blocks)
        {
            bool atTheStartOfADrag = !Interaction.DragUndoRecorded;
            if (atTheStartOfADrag)
            {
                RegisterUndoFor(blocks);
                Interaction.DragUndoRecorded = true;
            }
        }

        private void RegisterUndoFor(IList<Block> blocks)
        {
            var undoTargets = blocks.ToArray();
            if (undoTargets.Length > 0)
            {
                Undo.RegisterCompleteObjectUndo(undoTargets, "Adjust Block Position(s)");
            }
        }

        private void ApplyTheMovement(ref PointerEventInfo info, IList<Block> blocks)
        {
            float zoom = Mathf.Approximately(Flowchart.Zoom, 0f) ?
                1f :
                Flowchart.Zoom;
            Vector2 movementDelta = info.PanelDelta / zoom;

            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == null)
                {
                    Debug.LogWarning($"Block at index {i} was null. Skipping movement for this block.");
                    continue;
                }

                Block blockEl = blocks[i];
                Rect rect = blockEl._NodeRect;
                rect.position += movementDelta;
                blockEl._NodeRect = rect;
                Debug.Log($"Moved block '{blockEl.BlockName}'");
            }
        }

        public void OnLeftMouseUp(PointerEventInfo info, Event evt)
        {
            if (isDisposed || evt == null)
            {
                return;
            }

            if (Interaction.RootBlockToDrag == null)
            {
                return;
            }

            if (HyphlowEditorPreferences.useGridSnap)
            {
                flowchartContext.SnapBlocksToGrid();
            }
        }

    }

}