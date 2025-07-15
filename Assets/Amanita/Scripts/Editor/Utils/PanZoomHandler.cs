using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// For dragging the canvas and zooming the view
    /// </summary>
    public class PanZoomHandler : IEventHandler
    {
        public bool Handle(Event eventToHandle, FlowchartContext ctx)
        {
            switch (eventToHandle.type)
            {
                case EventType.MouseDrag:
                    return DragCanvas(eventToHandle, ctx);
                case EventType.ScrollWheel:
                    return HandleZoom(eventToHandle, ctx);
                default:
                    return false;
            }
        }

        protected bool DragCanvas(Event mouseEvent, FlowchartContext ctx)
        {
            Debug.Log("Handling canvas-dragging/panning");
            bool isPanTool = Tools.current == Tool.View && Tools.viewTool == ViewTool.Pan;
            bool isZoomTool = Tools.current == Tool.View && Tools.viewTool == ViewTool.Zoom;
            bool isAltDrag = mouseEvent.button == 0 && mouseEvent.alt;
            bool isMiddleDrag = mouseEvent.button == 2;
            bool isRightDrag = mouseEvent.button == 1;

            if (isPanTool || isAltDrag || isMiddleDrag || isRightDrag)
            {
                ctx.Flowchart.ScrollPos += mouseEvent.delta / ctx.Flowchart.Zoom;
                mouseEvent.Use();
                return true;
            }

            return false;
        }

        protected bool HandleZoom(Event eventToHandle, FlowchartContext ctx)
        {
            Debug.Log("Handling zoom");
            bool selectionBoxActive = ctx.SelectionBox.size != Vector2.zero;
            if (selectionBoxActive)
                return false; // Don't zoom when selection box is active

            Vector2 zoomCenter;
            zoomCenter.x = eventToHandle.mousePosition.x / ctx.Flowchart.Zoom / ctx.Position.width;
            zoomCenter.y = eventToHandle.mousePosition.y / ctx.Flowchart.Zoom / ctx.Position.height;
            zoomCenter *= ctx.Flowchart.Zoom;

            float zoomDelta = -eventToHandle.delta.y * 0.01f;

            DoZoom(ctx, zoomDelta, zoomCenter);
            eventToHandle.Use();
            return true;
        }

        protected void DoZoom(FlowchartContext ctx, float delta, Vector2 center)
        {
            var prevZoom = ctx.Flowchart.Zoom;
            ctx.Flowchart.Zoom += delta;
            ctx.Flowchart.Zoom = Mathf.Clamp(ctx.Flowchart.Zoom, MinZoom, MaxZoom);

            var deltaSize = ctx.Position.size / prevZoom - ctx.Position.size / ctx.Flowchart.Zoom;
            var offset = -Vector2.Scale(deltaSize, center);

            ctx.Flowchart.ScrollPos += offset;
            ctx.ForceRepaintCount = 1;
        }

        public virtual float MinZoom { get; set; } = 0.25f;
        public virtual float MaxZoom { get; set; } = 1f;
    }

    public interface IEventHandler
    {
        /// <summary>
        /// Try to consume this Event. Returns true if it did something.
        /// </summary>
        bool Handle(Event eventToHandle, FlowchartContext ctx);
    }

}