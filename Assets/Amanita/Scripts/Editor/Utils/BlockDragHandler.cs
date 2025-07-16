using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// Handles click‐and‐drag of selected blocks.
    /// </summary>
    public class BlockDragHandler : IUGUIEventHandler
    {
        public virtual bool Handle(Event mouseEvent, FlowchartContext flowchartCtx)
        {
            Validate(mouseEvent);
            Validate(flowchartCtx);

            switch (mouseEvent.type)
            {
                case EventType.MouseDown: return OnMouseDown(mouseEvent, flowchartCtx);
                case EventType.MouseDrag: return OnMouseDrag(mouseEvent, flowchartCtx);
                case EventType.MouseUp: return OnMouseButtonReleased(mouseEvent, flowchartCtx);
                default: return false;
            }
        }

        protected virtual void Validate(Event mouseEvent)
        {
            if (mouseEvent == null)
            {
                string errorMessage = "BlockDragHandler: Cannot work with null mouse event.";
                throw new InvalidOperationException(errorMessage);
            }
        }

        protected virtual void Validate(FlowchartContext ctx)
        {
            if (ctx == null)
            {
                string errorMessage = "BlockDragHandler: Cannot work with null Flowchart context.";
                throw new InvalidOperationException(errorMessage);
            }
        }

        protected virtual bool OnMouseDown(Event mouseEvent, FlowchartContext flowchartCtx)
        {
            flowchartCtx.StartDragPosition = (mouseEvent.mousePosition / flowchartCtx.Flowchart.Zoom) -
                flowchartCtx.Flowchart.ScrollPos;

            // Only left‐click (no Alt) on an already‐selected block starts a drag
            bool consumed = false;
            if (IsLeftMouseButton(mouseEvent) && !mouseEvent.alt)
            {
                var blockHit = flowchartCtx.HitTest(mouseEvent.mousePosition);
                bool onAlreadySelectedBlock = flowchartCtx.SelectedBlocks.Contains(blockHit);
                if (blockHit != null && onAlreadySelectedBlock)
                {
                    flowchartCtx.DragBlock = blockHit;
                    flowchartCtx.DragUndoRecorded = false;
                    flowchartCtx.HasDraggedSelected = false;
                    mouseEvent.Use();
                    consumed = true;
                }
            }
            return consumed;
        }

        public readonly string startBlockDragGroupName = "Block Drag";

        protected virtual bool IsLeftMouseButton(Event currentMouseEvent) => currentMouseEvent.button == 0;

        protected virtual bool OnMouseDrag(Event mouseEvent, FlowchartContext flowchartCtx)
        {
            bool consumed = false;

            if (IsLeftMouseButton(mouseEvent) && flowchartCtx.DragBlock != null)
            {
                bool atTheStartOfADrag = !flowchartCtx.DragUndoRecorded;
                if (atTheStartOfADrag)
                {
                    var blocks = flowchartCtx.SelectedBlocks.Cast<UnityEngine.Object>().ToArray();
                    Undo.RegisterCompleteObjectUndo(blocks, startBlockDragGroupName);

                    flowchartCtx.DragUndoRecorded = true;
                }

                MoveAllSelectedBlocks();
                void MoveAllSelectedBlocks()
                {
                    foreach (var elem in flowchartCtx.SelectedBlocks)
                    {
                        var elemRect = elem._NodeRect;
                        Vector2 movementSinceLastHandling = mouseEvent.delta;
                        elemRect.position += movementSinceLastHandling / flowchartCtx.Flowchart.Zoom;
                        elem._NodeRect = elemRect;
                    }
                }
                flowchartCtx.HasDraggedSelected = true;
                mouseEvent.Use();
                consumed = true;
            }

            return consumed;
        }

        protected virtual bool OnMouseButtonReleased(Event mouseEvent, FlowchartContext flowchartCtx)
        {
            // End drag: finalize positions & optional grid‐snap
            bool consumed = false;
            if (IsLeftMouseButton(mouseEvent) && flowchartCtx.DragBlock != null)
            {
                if (AmanitaEditorPreferences.useGridSnap)
                {
                    flowchartCtx.SnapBlocksToGrid();
                }
                flowchartCtx.DragBlock = null;
                flowchartCtx.HasDraggedSelected = false;
                flowchartCtx.DragUndoRecorded = false;
                mouseEvent.Use();
                consumed = true;
            }
            return consumed;
        }

    }
}