using System.Collections.Generic;
using UnityEngine;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// Handles drawing and finalizing a click-and-drag selection box.
    /// </summary>
    public class BoxSelectionHandler : IUGUIEventHandler
    {
        public virtual bool Handle(Event inputEvent, FlowchartContext ctx)
        {
            bool weWantToReact = IsLeftMouseButton(inputEvent) && !inputEvent.alt;

            if (weWantToReact)
            {
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
            else
            {
                return false;
            }
        }

        protected virtual bool OnMouseDown(Event inputEvent, FlowchartContext ctx)
        {
            Block blockBehindMouse = ctx.BlockHitInLastMouseDown;
            bool mouseIsOnEmptySpace = blockBehindMouse == null;

            if (mouseIsOnEmptySpace)
            {
                ctx.StartSelectionBoxPosition = inputEvent.mousePosition;
                ctx.SelectionBox = Rect.MinMaxRect
                (
                    inputEvent.mousePosition.x, inputEvent.mousePosition.y,
                    inputEvent.mousePosition.x, inputEvent.mousePosition.y
                );

                inputEvent.Use();
            }

            bool consumed = mouseIsOnEmptySpace;
            return consumed;
        }

        protected virtual bool IsLeftMouseButton(Event inputEvent) => inputEvent.button == 0;

        protected virtual bool OnMouseDrag(Event inputEvent, FlowchartContext ctx)
        {
            bool consumed = false;

            bool startedOnEmptySpace = !ctx.WeHitBlockInLastMouseDown;
            if (ctx.StartSelectionBoxPosition.x >= 0 && startedOnEmptySpace)
            {
                // Only register the drag as starting if we've moved past a certain threshold
                Vector2 start = ctx.StartSelectionBoxPosition;
                Vector2 current = inputEvent.mousePosition;
                Vector3 diff = start - current;
                diff.x = Mathf.Abs(diff.x);
                diff.y = Mathf.Abs(diff.y);
                bool movedFarEnough = diff.x > MinThreshold.x && diff.y > MinThreshold.y;
                
                if (!ctx.SelectionBoxDragOngoing && movedFarEnough)
                {
                    ctx.SelectionBoxDragOngoing = true;
                }

                if (ctx.SelectionBoxDragOngoing)
                {
                    UpdateSelectionBoxSize();
                    void UpdateSelectionBoxSize()
                    {
                        // Naturally, based off the drag start pos and the current mouse pos
                        Vector2 start = ctx.StartSelectionBoxPosition;
                        Vector2 current = inputEvent.mousePosition;

                        var bottomLeftCorner = Vector2.Min(start, current);
                        var topRightCorner = Vector2.Max(start, current);

                        ctx.SelectionBox = Rect.MinMaxRect
                        (
                            bottomLeftCorner.x, bottomLeftCorner.y,
                            topRightCorner.x, topRightCorner.y
                        );
                    }

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
            // Finalize selection, clear marquee
            bool consumed = false;
            bool releasedMouseOnValidSpot = ctx.StartSelectionBoxPosition.x >= 0;

            if (releasedMouseOnValidSpot && ctx.SelectionBoxDragOngoing)
            {
                Rect zoomBox = SelectionBoxInFlowchartSpace();
                Rect SelectionBoxInFlowchartSpace()
                {
                    // Since the zoom level can affect which Blocks are selected
                    Rect zoomBox = ctx.SelectionBox;
                    zoomBox.position -= ctx.Flowchart.ScrollPos * ctx.Flowchart.Zoom;
                    zoomBox.position /= ctx.Flowchart.Zoom;
                    zoomBox.size /= ctx.Flowchart.Zoom;
                    return zoomBox;
                }

                SelectBlocksOverlappedByBox();
                void SelectBlocksOverlappedByBox()
                {
                    ctx.Flowchart.ClearSelectedBlocks();
                    IList<Block> allBlocks = ctx.Flowchart.GetComponents<Block>();
                    foreach (var elem in allBlocks)
                    {
                        if (zoomBox.Overlaps(elem._NodeRect))
                            ctx.Flowchart.AddToSelection(elem);
                    }
                }

                ClearMarquee();
                void ClearMarquee()
                {
                    ctx.SelectionBox = default;
                    ctx.StartSelectionBoxPosition = default;
                }

                mouseEvent.Use();
                ctx.SelectionBoxDragOngoing = false;
                consumed = true;
            }

            return consumed;
        }
    }
}