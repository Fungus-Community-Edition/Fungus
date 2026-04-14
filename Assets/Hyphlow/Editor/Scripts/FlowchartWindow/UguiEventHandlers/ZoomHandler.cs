using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Handles zooming the Flowchart canvas via scroll wheel.
    /// </summary>
    public class ZoomHandler : IUGUIEventHandler
    {
        public bool Handle(Event eventToHandle, FlowchartContext ctx)
        {
            if (eventToHandle.type != EventType.ScrollWheel)
            {
                return false;
            }

            return HandleZoom(eventToHandle, ctx);
        }

        protected virtual bool HandleZoom(Event inputEvent, FlowchartContext ctx)
        {
            Flowchart flowchart = ctx.Flowchart;
            if (flowchart == null || ctx.Position.width <= 0f || ctx.Position.height <= 0f)
            {
                return false;
            }

            bool selectionBoxActive = ctx.Interaction.HasSelectionBox;
            bool shouldZoom = !(IsPanTool || selectionBoxActive);
            if (!shouldZoom)
            {
                return false;
            }

            Vector2 mousePosInWindowSpace = ctx.Document.ToWindowSpace(inputEvent.mousePosition);
            Vector2 zoomCenter = new Vector2(
                mousePosInWindowSpace.x / ctx.Position.width,
                mousePosInWindowSpace.y / ctx.Position.height) * flowchart.Zoom;

            float zoomDelta = -inputEvent.delta.y * 0.01f;

            DoZoom(ctx, zoomDelta, zoomCenter);
            inputEvent.Use();
            return true;
        }

        protected virtual bool IsPanTool => UnityEditor.Tools.current == UnityEditor.Tool.View &&
                                            UnityEditor.Tools.viewTool == UnityEditor.ViewTool.Pan;

        protected virtual void DoZoom(FlowchartContext ctx, float delta, Vector2 center)
        {
            Flowchart flowchart = ctx.Flowchart;
            if (flowchart == null)
            {
                return;
            }

            float prevZoom = flowchart.Zoom;
            flowchart.Zoom += delta;
            flowchart.Zoom = Mathf.Clamp(flowchart.Zoom, MinZoom, MaxZoom);

            Vector2 deltaSize = (ctx.Position.size / prevZoom) - (ctx.Position.size / flowchart.Zoom);
            Vector2 offset = -Vector2.Scale(deltaSize, center);

            flowchart.ScrollPos += offset;
            ctx.ForceRepaintCount = 1;
        }

        public virtual float MinZoom { get; set; } = 0.25f;
        public virtual float MaxZoom { get; set; } = 1f;
    }
}