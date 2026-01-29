using System;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles viewport panning in the UITK flowchart window by reacting to scroll-wheel drag deltas.
    /// </summary>
    public sealed class PanHandlerUitk : IFlowchartWindowModule, IScrollWheelDragResponder
    {
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
            if (isDisposed || flowchartContext.Flowchart == null)
            {
                Debug.LogWarning("PanHandlerUitk is disposed or Flowchart is null.");
                return;
            }

            if (direction.sqrMagnitude <= minDirectionMagnitude)
            {
                Debug.Log("Direction too small.");
                return;
            }

            Flowchart flowchart = flowchartContext.Flowchart;
            if (flowchart == null)
            {
                Debug.LogWarning("No Flowchart found.");
                return;
            }

            float zoom = Mathf.Approximately(flowchart.Zoom, 0f) ? 1f : flowchart.Zoom;
            Vector2 delta = direction / zoom;

            flowchart.ScrollPos -= delta;
            FlowchartWindowSignals.WindowPanned();
        }

        private static readonly float minDirectionMagnitude = 0.01f;

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