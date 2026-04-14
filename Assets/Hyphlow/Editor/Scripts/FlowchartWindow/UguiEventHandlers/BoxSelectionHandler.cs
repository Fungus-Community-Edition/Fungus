using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Handles drawing and finalizing a click-and-drag selection box.
    /// </summary>
    public class BoxSelectionHandler : IUGUIEventHandler
    {
        public virtual bool Handle(Event inputEvent, FlowchartContext ctx)
        {
            bool weWantToReact = IsLeftMouseButton(inputEvent) && !inputEvent.alt;

            if (!weWantToReact)
            {
                return false;
            }

            switch (inputEvent.type)
            {
                case EventType.MouseDown:
                    return OnMouseDown(inputEvent, ctx);
                case EventType.MouseDrag:
                    return OnMouseDrag(inputEvent, ctx);
                case EventType.MouseUp:
                    return OnMouseReleased(inputEvent, ctx);
                default:
                    return false;
            }
        }

        protected virtual bool OnMouseDown(Event inputEvent, FlowchartContext ctx)
        {
            var interaction = ctx.Interaction;
            bool mouseIsOnEmptySpace = !interaction.WeHitBlockInLastMouseDown;

            if (mouseIsOnEmptySpace)
            {
                interaction.SelectionBoxStartPos = inputEvent.mousePosition;
                interaction.SelectionBox = Rect.MinMaxRect(
                    inputEvent.mousePosition.x,
                    inputEvent.mousePosition.y,
                    inputEvent.mousePosition.x,
                    inputEvent.mousePosition.y);

                interaction.SelectionBoxDragOngoing = false;
                inputEvent.Use();
            }

            return mouseIsOnEmptySpace;
        }

        protected virtual bool IsLeftMouseButton(Event inputEvent) => inputEvent.button == 0;

        protected virtual bool OnMouseDrag(Event inputEvent, FlowchartContext ctx)
        {
            var interaction = ctx.Interaction;
            bool consumed = false;

            bool startedOnEmptySpace = !interaction.WeHitBlockInLastMouseDown;
            if (interaction.SelectionBoxStartPos.x >= 0 && startedOnEmptySpace)
            {
                Vector2 start = interaction.SelectionBoxStartPos;
                Vector2 current = inputEvent.mousePosition;
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

                    inputEvent.Use();
                    consumed = true;
                }
            }

            return consumed;
        }

        /// <summary>
        /// Minimum movement threshold for this to start registering a box selection
        /// </summary>
        public static readonly Vector2 MinThreshold = new Vector2(2, 2);

        protected virtual bool OnMouseReleased(Event mouseEvent, FlowchartContext ctx)
        {
            var interaction = ctx.Interaction;
            bool releasedMouseOnValidSpot = interaction.SelectionBoxStartPos.x >= 0;

            if (!(releasedMouseOnValidSpot && interaction.SelectionBoxDragOngoing && ctx.Flowchart != null))
            {
                return false;
            }

            Rect zoomBox = SelectionBoxInFlowchartSpace(interaction.SelectionBox, ctx.Flowchart);
            SelectBlocksOverlappedByBox(ctx, zoomBox);

            interaction.ResetSelectionBox();
            mouseEvent.Use();
            return true;
        }

        protected virtual Rect SelectionBoxInFlowchartSpace(Rect selectionBox, Flowchart flowchart)
        {
            Rect zoomBox = selectionBox;
            zoomBox.position -= flowchart.ScrollPos * flowchart.Zoom;
            zoomBox.position /= flowchart.Zoom;
            zoomBox.size /= flowchart.Zoom;
            return zoomBox;
        }

        protected virtual void SelectBlocksOverlappedByBox(FlowchartContext ctx, Rect zoomBox)
        {
            ctx.Selection.ClearBlocks();

            foreach (var block in EnumerateBlocks(ctx))
            {
                if (block != null && zoomBox.Overlaps(block._NodeRect))
                {
                    ctx.Selection.Add(block);
                }
            }
        }

        protected virtual IEnumerable<Block> EnumerateBlocks(FlowchartContext ctx)
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