using Amanita.EditorUtils;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles canvas panning via mouse drag.
    /// </summary>
    public class PanHandler : IUGUIEventHandler
    {
        public bool Handle(Event eventToHandle, FlowchartContext ctx)
        {
            if (eventToHandle.type != EventType.MouseDrag)
            {
                return false;
            }

            return DragCanvas(eventToHandle, ctx);
        }

        protected virtual bool DragCanvas(Event mouseEvent, FlowchartContext ctx)
        {
            Flowchart flowchart = ctx.Flowchart;
            if (flowchart == null)
            {
                return false;
            }

            InteractionState interaction = ctx.Interaction;
            bool correctDraggingInput = IsAltDragging(mouseEvent) ||
                                        IsMiddleDragging(mouseEvent) ||
                                        IsRightDragging(mouseEvent);
            bool otherDragOngoing = interaction.BlockDragOngoing || interaction.SelectionBoxDragOngoing;

            bool shouldDragCanvas = (IsPanTool || correctDraggingInput) && !otherDragOngoing;
            if (!shouldDragCanvas)
            {
                return false;
            }

            flowchart.ScrollPos += mouseEvent.delta / flowchart.Zoom;
            mouseEvent.Use();
            return true;
        }

        protected virtual bool IsPanTool => Tools.current == Tool.View && Tools.viewTool == ViewTool.Pan;

        protected virtual bool IsAltDragging(Event mouseEvent) => mouseEvent.button == 0 && mouseEvent.alt;
        protected virtual bool IsMiddleDragging(Event mouseEvent) => mouseEvent.button == 2;
        protected virtual bool IsRightDragging(Event mouseEvent) => mouseEvent.button == 1;
    }
}