using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Amanita.EditorUtils;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles click-and-drag of selected blocks in the UITK flowchart window.
    /// </summary>
    public sealed class BlockDragHandlerUitk : IFlowchartWindowModule,
        ILeftClickResponder, ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseUpResponder
    {
        public BlockDragHandlerUitk(FlowchartContext context)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        private readonly FlowchartContext flowchartContext;
        
        public void Initialize(FlowchartWindowUitk window)
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

        public void OnLeftClick(Vector2 position)
        {
            if (isDisposed)
            {
                return;
            }

            var interaction = flowchartContext.Interaction;
            interaction.BlockHitInLastMouseDown = FindTopmostBlock(position);
        }

        public void OnLeftMouseDragStarted(Vector2 startPos, Event evt)
        {
            if (isDisposed || evt == null || evt.alt)
            {
                return;
            }

            var flowchart = flowchartContext.Flowchart;
            var interaction = flowchartContext.Interaction;

            if (flowchart == null || !interaction.WeHitBlockInLastMouseDown)
            {
                return;
            }

            Vector2 mousePosInWindowSpace = flowchartContext.Document.ToWindowSpace(evt.mousePosition);
            interaction.StartDragPosition = mousePosInWindowSpace - flowchart.ScrollPos;

            Block blockHit = interaction.BlockHitInLastMouseDown;
            if (blockHit == null)
            {
                throw new InvalidOperationException("Hit metadata indicated a block, but none was found.");
            }

            interaction.RootBlockToDrag = blockHit;
            interaction.DragUndoRecorded = false;
            interaction.HasDraggedSelected = false;
        }

        public void OnLeftMouseDragged(Vector2 direction, Event evt)
        {
            if (isDisposed || evt == null || evt.alt)
            {
                return;
            }

            var flowchart = flowchartContext.Flowchart;
            var interaction = flowchartContext.Interaction;

            if (flowchart == null || interaction.RootBlockToDrag == null)
            {
                return;
            }

            var selection = flowchartContext.Selection.Blocks;
            bool atTheStartOfADrag = !interaction.DragUndoRecorded;
            if (atTheStartOfADrag)
            {
                RegisterUndoForDraggedBlocks();
                interaction.DragUndoRecorded = true;
                interaction.BlockDragOngoing = true;
            }

            void RegisterUndoForDraggedBlocks()
            {
                var undoTargets = selection.Cast<UnityObj>().ToArray();
                if (undoTargets.Length > 0)
                {
                    Undo.RegisterCompleteObjectUndo(undoTargets, "Adjust Block Position(s)");
                }
            }

            float zoom = Mathf.Approximately(flowchart.Zoom, 0f) ? 
                1f : 
                flowchart.Zoom;
            Vector2 movementDelta = direction / zoom;

            foreach (var block in selection)
            {
                if (block == null)
                {
                    continue;
                }

                Rect rect = block._NodeRect;
                rect.position += movementDelta;
                block._NodeRect = rect;
            }

            interaction.HasDraggedSelected = true;
        }

        public void OnLeftMouseUp(Vector2 pos, Event evt)
        {
            if (isDisposed || evt == null)
            {
                return;
            }

            var interaction = flowchartContext.Interaction;
            if (interaction.RootBlockToDrag == null)
            {
                return;
            }

            if (AmanitaEditorPreferences.useGridSnap)
            {
                flowchartContext.SnapBlocksToGrid();
            }

            interaction.ResetDragState();
        }

        private Block FindTopmostBlock(Vector2 mousePosition)
        {
            Flowchart flowchart = flowchartContext.Flowchart;
            if (flowchart == null)
            {
                return null;
            }

            var blocks = flowchartContext.Document.AllBlocks;
            if (blocks == null || blocks.Count == 0)
            {
                return null;
            }

            Block topmost = null;
            foreach (var blockEl in blocks)
            {
                if (blockEl == null)
                {
                    continue;
                }

                if (BlockHitTester.TryGetBlockWindowRect(blockEl, flowchart, out Rect rect) &&
                    rect.Contains(mousePosition))
                {
                    topmost = blockEl;
                }
            }

            return topmost;
        }

        private const string StartBlockDragGroupName = "Block Drag";
    }
}