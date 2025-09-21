using UnityEditor;
using UnityEngine;
using Amanita.VScripting.EditorUtils;
using Amanita.VScripting;

namespace Amanita.EditorUtils
{
    public class SingleSelectionHandler : IUGUIEventHandler
    {
        public bool Handle(Event inputEvent, FlowchartContext ctx)
        {
            bool weWantToReact = (IsLeftMouseButton(inputEvent) || IsRightMouseButton(inputEvent)) && !inputEvent.alt;

            if (weWantToReact)
            {
                switch (inputEvent.type)
                {
                    case EventType.MouseDown:
                        return OnMouseDown(inputEvent, ctx);
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

        protected static bool IsLeftMouseButton(Event inputEvent) => inputEvent.button == MouseButton.Left;
        protected static bool IsRightMouseButton(Event ev) => ev.button == MouseButton.Right;

        protected readonly static int leftMouseButton = 0;
        public readonly static string recordSelectedObject = "Select";

        protected virtual bool OnMouseDown(Event inputEvent, FlowchartContext flowchartCtx)
        {
            bool consumed = false;
            // ^With things as they are now, we'll always want this to be false. This way,
            // the other handlers can do their thing.

            if (IsLeftMouseButton(inputEvent))
            {
                bool atMostOneBlockSelected = flowchartCtx.SelectedBlockCount <= 1;
                var blockHit = flowchartCtx.BlockHitInLastMouseDown;
                bool hitNonSelectedBlock = blockHit != null && !flowchartCtx.Flowchart.SelectedBlocks.Contains(blockHit);
                bool multiSelect = IsMultiSelect(inputEvent);
                if ((atMostOneBlockSelected || hitNonSelectedBlock) && !multiSelect)
                {
                    // Need to avoid clearing when multiple blocks are selected. Otherwise, we'd
                    // be cancelling the multi select too early, keeping the user from
                    // dragging the blocks
                    flowchartCtx.Flowchart.ClearSelectedBlocks();
                }


                if (flowchartCtx.WeHitBlockInLastMouseDown)
                {
                    // Record for Undo
                    Undo.RecordObject(flowchartCtx.Flowchart, recordSelectedObject);

                    bool alreadySelected = flowchartCtx.SelectedBlocks.Contains(blockHit);
                    if (alreadySelected && multiSelect)
                    {
                        flowchartCtx.Flowchart.DeselectBlockNoCheck(blockHit);
                    }
                    else
                    {
                        flowchartCtx.Flowchart.AddToSelection(blockHit);
                    }
                }

            }

            return consumed;
        }

        protected static bool IsMultiSelect(Event e) => IsLeftMouseButton(e) && (e.control || e.command);

        protected virtual bool OnMouseReleased(Event inputEvent, FlowchartContext ctx)
        {
            var fc = ctx.Flowchart;
            var blockHit = ctx.BlockHitInLastMouseDown;
            bool hitEmpty = blockHit == null;
            bool hasDragRect = ctx.SelectionBox.size != Vector2.zero;

            if (hitEmpty && !hasDragRect && !IsMultiSelect(inputEvent))
            {
                FlowchartWindowSignals.EmptySpaceClicked();
            }
            else if (!hitEmpty)  // only when a real block was clicked
            {
                BlockSignals.BlockClicked(blockHit, inputEvent);
            }

            ctx.FcHost.UpdateBlockCollection();
            ctx.FcHost.Repaint();
            return false;
        }
    }
}