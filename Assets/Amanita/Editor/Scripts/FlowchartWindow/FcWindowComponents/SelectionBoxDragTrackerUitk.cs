using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles setting up the selection box during left-mouse drag operations. This does NOT draw the box;
    /// Drawing is handled by a separate component.
    /// </summary>
    public sealed class SelectionBoxDragTrackerUitk : IFlowchartWindowModule,
        ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder,
        IEmptySpaceLeftMouseDownResponder, IEmptySpaceLeftMouseUpResponder
    {
        public SelectionBoxDragTrackerUitk(FlowchartContext context)
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

        public void OnEmptySpaceLeftMouseDown(Vector2 pos, Event evt)
        {
            // We only want to start tracking when the drag starts on empty space, so...
            Debug.Log($"Box selection tracking enabled at {pos}");
            _shouldTrack = true;
        }

        bool _shouldTrack;

        public void OnEmptySpaceLeftMouseUp(Vector2 pos, Event evt)
        {
            Debug.Log($"Box selection tracking disabled at {pos}");
            // Let's delay it by a frame so that our DragEnded response can still run. Otherwise, 
            // _shouldTrack will be false by the time we get to DragEnded if the mouse up happens
            // before the drag ends, which it usually does.
            EditorApplication.delayCall += () => _shouldTrack = false;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
        }

        public void OnLeftMouseDragStarted(Vector2 startPos, Event evt)
        {
            if (isDisposed || evt == null || !_shouldTrack)
            {
                return;
            }

            var interaction = flowchartContext.Interaction;
            interaction.StartSelectionBoxPosition = startPos;
            interaction.SelectionBox = Rect.MinMaxRect(
                startPos.x,
                startPos.y,
                startPos.x,
                startPos.y);

            interaction.SelectionBoxDragOngoing = false;
            //Debug.Log($"Box selection started at {startPos}");
        }

        public void OnLeftMouseDragged(Vector2 _, Event evt)
        {
            if (isDisposed || evt == null || !_shouldTrack)
            {
                return;
            }

            var interaction = flowchartContext.Interaction;
            Vector2 start = interaction.StartSelectionBoxPosition;
            Vector2 current = evt.mousePosition;
            Vector2 diff = new Vector2(Mathf.Abs(start.x - current.x), Mathf.Abs(start.y - current.y));
            bool movedFarEnough = diff.x > MinThreshold.x && diff.y > MinThreshold.y;

            if (!interaction.SelectionBoxDragOngoing && movedFarEnough)
            {
                interaction.SelectionBoxDragOngoing = true;
            }

            if (interaction.SelectionBoxDragOngoing)
            {
                Vector2 bottomLeftCorner = Vector2.Min(start, current);
                Vector2 topRightCorner = Vector2.Max(start, current);

                interaction.SelectionBox = Rect.MinMaxRect(
                    bottomLeftCorner.x,
                    bottomLeftCorner.y,
                    topRightCorner.x,
                    topRightCorner.y);

                //UpdateSelectionDuringDrag(flowchartContext, interaction.SelectionBox);
            }

            //Debug.Log($"Selection box drag tracker: Box selection dragged to {current}");

        }

        public void OnLeftMouseDragEnded(Vector2 endPos, Event evt)
        {
            if (isDisposed || evt == null || !_shouldTrack)
            {
                return;
            }

            var interaction = flowchartContext.Interaction;
            bool releasedMouseOnValidSpot = interaction.StartSelectionBoxPosition.x >= 0;
            bool validFc = flowchartContext.Flowchart != null;
            if (!(releasedMouseOnValidSpot && interaction.SelectionBoxDragOngoing && validFc))
            {
                return;
            }

            SelectBlocksOverlappedByBox(flowchartContext, interaction.SelectionBox);

            interaction.ResetSelectionBox();
            interaction.SelectionBoxDragOngoing = false;
            _shouldTrack = false;
            Debug.Log($"Box selection ended at {endPos}");
            Debug.Log($"Zoom: {flowchartContext.Flowchart.Zoom}, ScrollPos: {flowchartContext.Flowchart.ScrollPos}");
        }

        /// <summary>
        /// Minimum movement threshold for this to start registering a box selection
        /// </summary>
        public static readonly Vector2 MinThreshold = new Vector2(2, 2);

        private static void UpdateBlockSelection(FlowchartContext ctx, Rect selectionBox)
        {
            Flowchart flowchart = ctx.Flowchart;
            if (flowchart == null)
            {
                return;
            }

            Rect zoomSelectionBox = ToFlowchartSpace(selectionBox, flowchart);

            ctx.Selection.ClearBlocks();
            foreach (var block in EnumerateBlocks(ctx))
            {
                if (block == null)
                {
                    continue;
                }

                if (zoomSelectionBox.Overlaps(block._NodeRect))
                {
                    ctx.Selection.Add(block);
                }
            }
        }

        private static void SelectBlocksOverlappedByBox(FlowchartContext ctx, Rect selectionBox)
        {
            UpdateBlockSelection(ctx, selectionBox);

            int blockCount = ctx.Selection.BlockCount;
            if (blockCount == 1)
            {
                BlockSignals.BlockSelected?.Invoke(ctx.Selection.Blocks[0]);
                Debug.Log("1 block selected via box selection.");
            }
            else if (blockCount > 1)
            {
                BlockSignals.MultiBlocksSelected?.Invoke(ctx.Selection.Blocks);
                Debug.Log($"{blockCount} blocks selected via box selection.");
            }
            else
            {
                FlowchartWindowSignals.EmptySpaceClicked?.Invoke(Vector2.zero);
            }
        }

        private static Rect ToFlowchartSpace(Rect selectionBox, Flowchart flowchart)
        {
            float zoom = Mathf.Approximately(flowchart.Zoom, 0f) ? 
                1f : 
                flowchart.Zoom;

            Rect zoomSelectionBox = selectionBox;
            zoomSelectionBox.position -= flowchart.ScrollPos * zoom;
            zoomSelectionBox.position /= zoom;
            zoomSelectionBox.size /= zoom;

            return zoomSelectionBox;
        }

        private static IEnumerable<Block> EnumerateBlocks(FlowchartContext ctx)
        {
            var blocks = ctx.Document.AllBlocks;
            if (blocks != null && blocks.Count > 0)
            {
                return blocks;
            }

            return ctx.Flowchart != null ? ctx.Flowchart.GetComponents<Block>() : Array.Empty<Block>();
        }

        
    }
}