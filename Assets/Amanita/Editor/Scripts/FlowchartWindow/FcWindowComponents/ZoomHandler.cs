using System;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Handles zooming the active Flowchart in response to non-click scroll-wheel input.
    /// Bounds are configurable and default to 1f so that callers can opt in to wider ranges.
    /// </summary>
    public sealed class ZoomHandler : IFlowchartWindowModule,
        IScrollWheelMoveResponder,
        IFlowchartChangeResponder
    {
        public int Priority { get; set; } = 0;
        private readonly FlowchartContext flowchartContext;
        private FlowchartWindow owner;
        private bool isDisposed;
        private float minZoom;
        private float maxZoom;

        private const float DefaultZoom = 1f;
        private const float ZoomStepPerDelta = 0.1f;
        private const float MinAllowedZoom = 0.5f;

        public ZoomHandler(FlowchartContext context, float minZoomLevel = DefaultZoom, float maxZoomLevel = DefaultZoom)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
            ApplyZoomBounds(minZoomLevel, maxZoomLevel);
        }

        public float MinZoom
        {
            get { return minZoom; }
            set { ApplyZoomBounds(value, maxZoom); }
        }

        public float MaxZoom
        {
            get { return maxZoom; }
            set { ApplyZoomBounds(minZoom, value); }
        }

        public void Initialize(FlowchartWindow window)
        {
            owner = window ?? throw new ArgumentNullException(nameof(window));
        }

        public void OnScrollWheelMoved()
        {
            if (isDisposed)
            {
                return;
            }

            Event currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type != EventType.ScrollWheel)
            {
                return;
            }

            Vector2 scrollDelta = currentEvent.delta.normalized; // Normalized to make sure zoom step is consistent
            if (scrollDelta.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            ApplyZoomDelta(-scrollDelta.y * ZoomStepPerDelta);
            FlowchartWindowSignals.ZoomChanged?.Invoke(flowchartContext.Flowchart?.Zoom ?? DefaultZoom);
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart current)
        {
            if (isDisposed || current == null)
            {
                return;
            }

            float normalized = NormalizeZoom(current.Zoom);
            float clamped = Mathf.Clamp(normalized, minZoom, maxZoom);
            if (!Mathf.Approximately(normalized, clamped))
            {
                current.Zoom = clamped;
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            owner = null;
        }

        private void ApplyZoomDelta(float delta)
        {
            if (Mathf.Approximately(delta, 0f))
            {
                return;
            }

            Flowchart flowchart = flowchartContext.Flowchart;
            if (flowchart == null)
            {
                return;
            }

            float currentZoom = NormalizeZoom(flowchart.Zoom);
            float targetZoom = Mathf.Clamp(currentZoom + delta, minZoom, maxZoom);

            if (Mathf.Approximately(targetZoom, currentZoom))
            {
                return;
            }

            flowchart.Zoom = targetZoom;
            owner?.Repaint();
        }

        private void ApplyZoomBounds(float minCandidate, float maxCandidate)
        {
            float sanitizedMin = Mathf.Max(MinAllowedZoom, minCandidate);
            float sanitizedMax = Mathf.Max(MinAllowedZoom, maxCandidate);

            if (sanitizedMax < sanitizedMin)
            {
                float temp = sanitizedMin;
                sanitizedMin = sanitizedMax;
                sanitizedMax = temp;
            }

            minZoom = sanitizedMin;
            maxZoom = sanitizedMax;
        }

        private static float NormalizeZoom(float zoomValue)
        {
            return Mathf.Approximately(zoomValue, 0f) ?
                DefaultZoom :
                zoomValue;
        }
    }
}