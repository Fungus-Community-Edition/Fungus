using System;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles viewport panning in the UITK flowchart window by reacting to scroll-wheel drag deltas.
    /// </summary>
    public sealed class PanHandlerUitk : IFlowchartWindowModule, IScrollWheelDragResponder, IRightMouseDragResponder
    {
        public int Priority { get; set; } = 0;
        private FlowchartContext flowchartContext;
        private FlowchartWindowUitk owner;
        private bool isDisposed;

        public PanHandlerUitk(FlowchartContext context)
        {
            flowchartContext = context;
        }

        public void Initialize(FlowchartWindowUitk window)
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

        public void OnRightMouseDragged(Vector2 direction, Event evt)
        {
            if (!evt.shift)
            {
                return;
            }

            OnDragInput(direction);
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