using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
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

            // Note: We only want to react to the mouse movement while the left mouse button
            // is pressed (and while alt is NOT pressed). Checking for that here keeps us
            // from having to check in our OnMouse funcs
            bool weWantToReact = IsLeftMouseButton(mouseEvent) && !mouseEvent.alt;

            if (weWantToReact)
            {
                switch (mouseEvent.type)
                {
                    case EventType.MouseDown: return OnMouseDown(mouseEvent, flowchartCtx);
                    case EventType.MouseDrag: return OnMouseDrag(mouseEvent, flowchartCtx);
                    case EventType.MouseUp: return OnMouseButtonReleased(mouseEvent, flowchartCtx);
                    default: return false;
                }
            }
            else
            {
                return false;
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
            bool consumed = false;

            if (flowchartCtx.WeHitBlockInLastMouseDown)
            {
                Vector2 mousePosInWindowSpace = (mouseEvent.mousePosition / flowchartCtx.Flowchart.Zoom);
                flowchartCtx.StartDragPosition = mousePosInWindowSpace - flowchartCtx.Flowchart.ScrollPos;

                var blockHit = flowchartCtx.BlockHitInLastMouseDown; 
                if (blockHit == null)
                {
                    string errorMessage = "Last selected mouse down registered as hitting block, yet there is none under the cursor.";
                    throw new System.InvalidOperationException(errorMessage);
                }

                flowchartCtx.RootBlockToDrag = blockHit;
                flowchartCtx.DragUndoRecorded = false;
                flowchartCtx.HasDraggedSelected = false;
                mouseEvent.Use();
                consumed = true;
                
            }
            
            return consumed;
        }

        public readonly string startBlockDragGroupName = "Block Drag";

        protected virtual bool IsLeftMouseButton(Event currentMouseEvent) => currentMouseEvent.button == 0;

        protected virtual bool OnMouseDrag(Event mouseEvent, FlowchartContext flowchartCtx)
        {
            bool consumed = false;

            if (flowchartCtx.RootBlockToDrag != null)
            {
                bool atTheStartOfADrag = !flowchartCtx.DragUndoRecorded;
                if (atTheStartOfADrag)
                {
                    var blocks = flowchartCtx.SelectedBlocks.Cast<UnityEngine.Object>().ToArray();
                    Undo.RegisterCompleteObjectUndo(blocks, startBlockDragGroupName);

                    flowchartCtx.DragUndoRecorded = true;
                    flowchartCtx.BlockDragOngoing = true;
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

            if (flowchartCtx.RootBlockToDrag != null)
            {
                if (AmanitaEditorPreferences.useGridSnap)
                {
                    flowchartCtx.SnapBlocksToGrid();
                }
                flowchartCtx.RootBlockToDrag = null;
                flowchartCtx.HasDraggedSelected = false;
                flowchartCtx.DragUndoRecorded = false;
                flowchartCtx.BlockDragOngoing = false;
                mouseEvent.Use();
                consumed = true;
            }

            return consumed;
        }

    }
}