using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class CanvasUtils
    {
        /// <summary>
        /// Determines whether a mouse position (in screen space) is over
        /// a node's Rect (in world‐space) given scroll & zoom.
        /// </summary>
        public static bool IsPointOverNode(Rect nodeRect, Vector2 scrollPos,
            float zoom, Vector2 mousePosition)
        {
            Vector2 canvasMousePos = mousePosition / zoom;

            // This will be in canvas coords
            Rect canvasRect = new Rect(
                nodeRect.position + scrollPos,
                nodeRect.size);

            // Hit test
            return canvasRect.Contains(canvasMousePos);
        }

        /// <summary>
        /// Calculates the scroll‐position delta when zooming around a pivot.
        /// viewSize is in screen pixels, pivotNormalized ∈ [0,1]² (0=top/left, 1=bottom/right).
        /// </summary>
        public static Vector2 CalculateScrollDelta(Vector2 viewSize, float oldZoom,
            float newZoom, Vector2 pivotNormalized)
        {
            Vector2 canvasSizeBeforeZoom = viewSize / oldZoom;
            Vector2 canvasSizeAfterZoom = viewSize / newZoom;

            Vector2 sizeDiff = canvasSizeBeforeZoom - canvasSizeAfterZoom;
            // ^This decides how much the world “shrinks”/“grows”

            // Pivot determines which corner stays fixed
            return -Vector2.Scale(sizeDiff, pivotNormalized);
        }
    }
}