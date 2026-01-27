using UnityEngine;
using Amanita.VScripting.EditorUtils;
using Amanita.VScripting;

namespace Amanita.EditorUtils
{
    public class HitDetectionHandler : IUGUIEventHandler
    {
        public bool Handle(Event eventToHandle, FlowchartContext ctx)
        {
            bool weWantToReact = eventToHandle.type == EventType.MouseDown && eventToHandle.button == leftMouseButton;
            if (weWantToReact)
            {
                return OnMouseDown(eventToHandle, ctx);
            }

            return false;
        }

        protected static readonly int leftMouseButton = 0;

        protected virtual bool OnMouseDown(Event inputEvent, FlowchartContext flowchartCtx)
        {
            if (flowchartCtx == null)
            {
                return false;
            }

            flowchartCtx.Interaction.ResetSelectionBox();
            Block blockHit = flowchartCtx.Document.TopmostBlockOverlapping(inputEvent.mousePosition);
            flowchartCtx.Interaction.BlockHitInLastMouseDown = blockHit;

            return false;
            // Prep work only; other handlers still need the event.
        }
    }
}