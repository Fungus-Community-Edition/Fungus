using System;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Handles viewport panning in the UITK flowchart window by reacting to scroll-wheel drag deltas.
    /// </summary>
    public sealed class PanHandler : IFlowchartWindowModule, IScrollWheelDragResponder, IRightMouseDragResponder
    {
        public int Priority { get; set; } = 0;
        private FlowchartContext flowchartContext;
        private FlowchartWindow owner;
        private bool isDisposed;

        public PanHandler(FlowchartContext context)
        {
            flowchartContext = context;
        }

        public void Initialize(FlowchartWindow window)
        {
            owner = window != null ? 
                window : 
                throw new ArgumentNullException(nameof(window));
        }

        public void OnScrollWheelDragged(Vector2 direction)
        {
            OnDragInput(direction);
        }

        private void OnDragInput(Vector2 direction)
        {
            Flowchart flowchart = flowchartContext.Flowchart;
            if (isDisposed || flowchart == null)
            {
                Debug.LogWarning("PanHandlerUitk is disposed or Flowchart is null.");
                return;
            }

            HandlePanning(direction);
        }

        private void HandlePanning(Vector2 direction)
        {
            if (direction.sqrMagnitude <= minDirectionMagnitude)
            {
                Debug.Log("Direction too small.");
                return;
            }

            Flowchart flowchart = flowchartContext.Flowchart;
            float zoom = Mathf.Approximately(flowchart.Zoom, 0f) ? 1f : flowchart.Zoom;
            Vector2 directionAdjusted = direction / zoom;

            flowchart.ScrollPos -= directionAdjusted;
            FlowchartWindowSignals.WindowPanned();
        }

        private static readonly float minDirectionMagnitude = 0.01f;

        public void OnRightMouseDragged(PointerEventInfo info, Event evt)
        {
            if (!evt.shift)
            {
                return;
            }

            OnDragInput(info.PanelDelta);
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            owner = null;
            flowchartContext = null;
        }

        
    }
}