using UnityEngine;
using Amanita.VScripting.EditorUtils;
using Amanita.VScripting;

namespace Amanita.EditorUtils
{
    public class HitDetectionHandler : IUGUIEventHandler
    {
        public bool Handle(Event eventToHandle, FlowchartContext ctx)
        {
            bool weWantToReact = eventToHandle.MouseDown() && eventToHandle.LeftMouseButton();
            if (weWantToReact)
            {
                return OnMouseDown(eventToHandle, ctx);
            }

            return false;
        }

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