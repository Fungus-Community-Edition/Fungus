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
            else
            {
                return false;
            }
        }

        protected static readonly int leftMouseButton = 0;

        protected virtual bool OnMouseDown(Event inputEvent, FlowchartContext flowchartCtx)
        {
            flowchartCtx.SelectionBox = Rect.zero;
            Block blockHit = flowchartCtx.TopmostBlockOverlapping(inputEvent.mousePosition);
            flowchartCtx.BlockHitInLastMouseDown = blockHit;
            string blockHitName = "null";

            if (blockHit != null)
            {
                blockHitName = blockHit.BlockName;
            }

            return false;
            // ^We won't want this to get in the way of other event handlers doing their thing.
            // This one is just a prepper for the others.

        }
    }
}