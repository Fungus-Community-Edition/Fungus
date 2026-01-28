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

            bool weWantToReact = IsLeftMouseButton(mouseEvent) && !mouseEvent.alt;

            if (!weWantToReact)
            {
                return false;
            }

            switch (mouseEvent.type)
            {
                case EventType.MouseDown:
                    return OnMouseDown(mouseEvent, flowchartCtx);
                case EventType.MouseDrag:
                    return OnMouseDrag(mouseEvent, flowchartCtx);
                case EventType.MouseUp:
                    return OnMouseButtonReleased(mouseEvent, flowchartCtx);
                default:
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

        protected virtual bool OnMouseDown(Event mouseEvent, FlowchartContext ctx)
        {
            var flowchart = ctx.Flowchart;
            var interaction = ctx.Interaction;

            if (flowchart == null || !interaction.WeHitBlockInLastMouseDown)
            {
                return false;
            }

            Vector2 mousePosInWindowSpace = ctx.Document.ToWindowSpace(mouseEvent.mousePosition);
            interaction.StartDragPosition = mousePosInWindowSpace - flowchart.ScrollPos;

            var blockHit = interaction.BlockHitInLastMouseDown;
            if (blockHit == null)
            {
                throw new InvalidOperationException("Hit metadata indicated a block, but none was found.");
            }

            interaction.RootBlockToDrag = blockHit;
            interaction.DragUndoRecorded = false;
            interaction.HasDraggedSelected = false;

            mouseEvent.Use();
            return true;
        }

        public readonly string startBlockDragGroupName = "Block Drag";

        protected virtual bool IsLeftMouseButton(Event currentMouseEvent) => currentMouseEvent.button == 0;

        protected virtual bool OnMouseDrag(Event mouseEvent, FlowchartContext ctx)
        {
            var flowchart = ctx.Flowchart;
            var interaction = ctx.Interaction;

            if (flowchart == null || interaction.RootBlockToDrag == null)
            {
                return false;
            }

            var selection = ctx.Selection.Blocks;
            bool atTheStartOfADrag = !interaction.DragUndoRecorded;
            if (atTheStartOfADrag)
            {
                var undoTargets = selection.Cast<UnityEngine.Object>().ToArray();
                if (undoTargets.Length > 0)
                {
                    Undo.RegisterCompleteObjectUndo(undoTargets, startBlockDragGroupName);
                }

                interaction.DragUndoRecorded = true;
                interaction.BlockDragOngoing = true;
            }

            foreach (var block in selection)
            {
                if (block == null)
                {
                    continue;
                }

                Rect rect = block._NodeRect;
                rect.position += mouseEvent.delta / flowchart.Zoom;
                block._NodeRect = rect;
            }

            interaction.HasDraggedSelected = true;
            mouseEvent.Use();
            return true;
        }

        protected virtual bool OnMouseButtonReleased(Event mouseEvent, FlowchartContext ctx)
        {
            var interaction = ctx.Interaction;

            if (interaction.RootBlockToDrag == null)
            {
                return false;
            }

            if (AmanitaEditorPreferences.useGridSnap)
            {
                ctx.SnapBlocksToGrid();
            }

            interaction.ResetDragState();
            mouseEvent.Use();
            return true;
        }
    }
}