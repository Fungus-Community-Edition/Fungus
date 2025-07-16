using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.EditorUtils
{
    public class SelectionHandler : IUGUIEventHandler
    {
        public bool Handle(Event e, FlowchartContext ctx)
        {
            // Only on left‐mouse down
            if (e.type != EventType.MouseDown || e.button != 0)
                return false;

            var hit = ctx.HitTest(e.mousePosition);

            if (hit != null && !e.alt)
            {
                // Select that block, but DO NOT consume the event
                ctx.Flowchart.ClearSelectedBlocks();
                ctx.Flowchart.AddSelectedBlock(hit);
                // Let the event fall through so BlockDragHandler can pick it up
                return false;
            }

            // Clicked on empty space → clear selection *and* consume
            Undo.RecordObject(ctx.Flowchart, "Select");
            ctx.Flowchart.ClearSelectedBlocks();
            e.Use();
            //ctx.Window.Repaint();
            return true;
        }



        //public virtual bool Handle(Event eventToHandle, FlowchartContext ctx)
        //{
        //    bool consumed = false;
        //    bool leftMouseButton = eventToHandle.button == 0;
        //    bool shouldConsider = eventToHandle.type == EventType.MouseDown && leftMouseButton;
        //    if (shouldConsider)
        //    {
        //        eventToHandle.Use();

        //        var blockHit = ctx.HitTest(eventToHandle.mousePosition);
        //        if (blockHit != null && !eventToHandle.alt)
        //        {
        //            ctx.Flowchart.SelectedBlocks.Clear();
        //            ctx.Flowchart.AddSelectedBlock(blockHit);
        //            Undo.RecordObject(ctx.Flowchart, "Select");
        //            eventToHandle.Use();
        //            consumed = true;
        //        }
        //        else
        //        {
        //            ctx.Flowchart.SelectedBlocks.Clear();
        //            Undo.RecordObject(ctx.Flowchart, "Clear Block Selection");
        //        }

        //        ctx.Window.Repaint();

        //    }
        //    return consumed;
        //}
    }
}