using UnityEditor;
using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// For dragging the canvas and zooming the view
    /// </summary>
    public class PanZoomHandler : IUGUIEventHandler
    {
        public bool Handle(Event eventToHandle, FlowchartContext ctx)
        {
            bool consumed = false;
            switch (eventToHandle.type)
            {
                case EventType.MouseDrag:
                    consumed = DragCanvas(eventToHandle, ctx); break;
                case EventType.ScrollWheel:
                    consumed = HandleZoom(eventToHandle, ctx); break;
                default:
                    break;
            }

            return consumed;
        }

        protected bool DragCanvas(Event mouseEvent, FlowchartContext ctx)
        {

            bool consumed = false;
            bool correctDraggingInput = IsAltDragging(mouseEvent) ||
                IsMiddleDragging(mouseEvent) || IsRightDragging(mouseEvent);
            bool otherDragOngoing = ctx.BlockDragOngoing || ctx.SelectionBoxDragOngoing;

            bool shouldDragCanvas = (IsPanTool || correctDraggingInput) && !otherDragOngoing;
            if (shouldDragCanvas)
            {
                ctx.Flowchart.ScrollPos += mouseEvent.delta / ctx.Flowchart.Zoom;
                mouseEvent.Use();
                consumed = true;
            }

            return consumed;
        }

        protected virtual bool IsPanTool
        {
            get
            {
                return Tools.current == Tool.View && Tools.viewTool == ViewTool.Pan;
            }
        }

        protected virtual bool IsAltDragging(Event mouseEvent)
        {
            return mouseEvent.button == 0 && mouseEvent.alt;
        }

        protected virtual bool IsMiddleDragging(Event mouseEvent)
        {
            return mouseEvent.button == 2;
        }

        protected virtual bool IsRightDragging(Event mouseEvent)
        {
            return mouseEvent.button == 1;
        }

        protected bool HandleZoom(Event inputEvent, FlowchartContext flowchartCtx)
        {
            bool consumed = false;
            bool selectionBoxActive = flowchartCtx.SelectionBox.size != Vector2.zero;
            bool shouldZoom = !(IsPanTool || selectionBoxActive);
            if (shouldZoom)
            {
                Vector2 zoomCenter;
                Vector2 mousePosInWindowSpace = inputEvent.mousePosition / flowchartCtx.Flowchart.Zoom;
                zoomCenter.x = mousePosInWindowSpace.x / flowchartCtx.Position.width;
                zoomCenter.y = mousePosInWindowSpace.y / flowchartCtx.Position.height;
                zoomCenter *= flowchartCtx.Flowchart.Zoom;

                float zoomDelta = -inputEvent.delta.y * 0.01f;

                DoZoom(flowchartCtx, zoomDelta, zoomCenter);
                inputEvent.Use();
                consumed = true;
            }
            return consumed;
        }

        protected void DoZoom(FlowchartContext ctx, float delta, Vector2 center)
        {
            var prevZoom = ctx.Flowchart.Zoom;
            ctx.Flowchart.Zoom += delta;
            ctx.Flowchart.Zoom = Mathf.Clamp(ctx.Flowchart.Zoom, MinZoom, MaxZoom);

            var deltaSize = (ctx.Position.size / prevZoom) - (ctx.Position.size / ctx.Flowchart.Zoom);
            var offset = -Vector2.Scale(deltaSize, center);

            ctx.Flowchart.ScrollPos += offset;
            ctx.ForceRepaintCount = 1;
        }

        public virtual float MinZoom { get; set; } = 0.25f;
        public virtual float MaxZoom { get; set; } = 1f;
    }

    

}