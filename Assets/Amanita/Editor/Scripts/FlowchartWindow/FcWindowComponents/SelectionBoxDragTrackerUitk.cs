using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Handles setting up the selection box during left-mouse drag operations. This does NOT draw the box;
    /// Drawing is handled by a separate component.
    /// </summary>
    public sealed class SelectionBoxDragTrackerUitk : IFlowchartWindowModule,
        ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder,
        IEmptySpaceLeftMouseDownResponder, IEmptySpaceLeftMouseUpResponder
    {
        public int Priority { get; set; } = 0;
        public SelectionBoxDragTrackerUitk(FlowchartContext context)
        {
            fcContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        private readonly FlowchartContext fcContext;
        
        public void Initialize(FlowchartWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            isDisposed = false;
        }

        private bool isDisposed;

        public void OnEmptySpaceLeftMouseDown(PointerEventInfo info, Event evt)
        {
            if (fcContext.Selection.BlockCount > 0)
            {
                _shouldTrack = false;
                return; // This can happen right after adding a Block, which selects it.
                        // We don't want to start a box selection in that case.
            }
            _shouldTrack = true;
        }

        private bool _shouldTrack;

        public void OnEmptySpaceLeftMouseUp(PointerEventInfo info, Event evt)
        {
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

        public void OnLeftMouseDragStarted(PointerEventInfo info, Event evt)
        {
            if (fcContext.Selection.BlockCount > 0)
            {
                _shouldTrack = false;
                return; // This can happen right after adding a Block, which selects it.
                        // We don't want to start a box selection in that case.
            }
            if (isDisposed || evt == null || !_shouldTrack)
            {
                return;
            }

            var interaction = fcContext.Interaction;
            interaction.SelectionBoxStartPos = info.FlowchartPosition;
            interaction.SelectionBox = Rect.MinMaxRect(
                info.FlowchartPosition.x,
                info.FlowchartPosition.y,
                info.FlowchartPosition.x,
                info.FlowchartPosition.y);

            interaction.SelectionBoxDragOngoing = false;
        }

        public void OnLeftMouseDragged(PointerEventInfo info, Event evt)
        {
            if (isDisposed || evt == null || !_shouldTrack)
            {
                return;
            }

            var interaction = fcContext.Interaction;
            Vector2 start = interaction.SelectionBoxStartPos;
            Vector2 current = info.FlowchartPosition;
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
            }
        }

        public void OnLeftMouseDragEnded(PointerEventInfo info, Event evt)
        {
            if (isDisposed || evt == null || !_shouldTrack)
            {
                return;
            }

            var interaction = fcContext.Interaction;
            bool releasedMouseOnValidSpot = interaction.SelectionBoxStartPos.x >= 0;
            bool validFc = fcContext.Flowchart != null;
            if (!(releasedMouseOnValidSpot && interaction.SelectionBoxDragOngoing && validFc))
            {
                return;
            }

            SelectBlocksOverlappedByBox(fcContext, interaction.SelectionBox);

            interaction.ResetSelectionBox();
            interaction.SelectionBoxDragOngoing = false;
            _shouldTrack = false;
            //Debug.Log($"Box selection ended at {info.FlowchartPosition}");
            //Debug.Log($"Zoom: {fcContext.Flowchart.Zoom}, ScrollPos: {fcContext.Flowchart.ScrollPos}");
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