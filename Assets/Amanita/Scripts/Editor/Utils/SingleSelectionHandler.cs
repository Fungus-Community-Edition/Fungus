using UnityEditor;
using UnityEngine;

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

        protected virtual bool IsLeftMouseButton(Event inputEvent) => inputEvent.button == MouseButton.Left;
        protected virtual bool IsRightMouseButton(Event ev) => ev.button == MouseButton.Right;

        protected readonly static int leftMouseButton = 0;
        public readonly static string recordSelectedObject = "Select";

        protected virtual bool OnMouseDown(Event inputEvent, FlowchartContext flowchartCtx)
        {
            bool consumed = false;
            // ^With things as they are now, we'll always want this to be false. This way,
            // the other handlers can do their thing.

            if (IsLeftMouseButton(inputEvent))
            {
                bool atMostOneBlockSelected = flowchartCtx.SelectedBlocks.Count <= 1;
                var blockHit = flowchartCtx.BlockHitInLastMouseDown;
                bool hitNonSelectedBlock = blockHit != null && !flowchartCtx.Flowchart.SelectedBlocks.Contains(blockHit);
                if (atMostOneBlockSelected || hitNonSelectedBlock)
                {
                    // Need to avoid clearing when multiple blocks are selected. Otherwise, we'd
                    // be cancelling the multi select too early, keeping the user from
                    // dragging the blocks
                    Debug.Log("Single selection handler clearing selected blocks");
                    flowchartCtx.Flowchart.ClearSelectedBlocks();
                }

                if (flowchartCtx.WeHitBlockInLastMouseDown)
                {
                    // Record for Undo
                    Undo.RecordObject(flowchartCtx.Flowchart, recordSelectedObject);
                    flowchartCtx.Flowchart.AddSelectedBlock(blockHit);
                }

            }

            return consumed;
        }

        protected virtual bool OnMouseReleased(Event inputEvent, FlowchartContext flowchartCtx)
        {
            bool consumed = false;
            Flowchart fc = flowchartCtx.Flowchart;
            Block blockHit = flowchartCtx.BlockHitInLastMouseDown;
            bool hitEmptySpace = blockHit == null;
            var window = flowchartCtx.Window;
            if (hitEmptySpace)
            {
                flowchartCtx.Flowchart.ClearSelectedBlocks();
            }
            else
            {
                FlowchartWindow.SetBlockForInspector(flowchartCtx.Flowchart, blockHit);
                if (fc.SelectedBlocks.Count == 0)
                {
                    fc.AddSelectedBlock(blockHit);
                }
            }

            window.UpdateBlockCollection();
            window.Repaint();

            return consumed;
        }
    }
}