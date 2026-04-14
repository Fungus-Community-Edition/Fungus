using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    static class RectExtensions
    {
        // Extension to convert window-space to flowchart-space
        public static Vector2 PointToNormalized(this Rect viewRect, Vector2 windowPoint)
        {
            // Inverse of Zoom + Scroll
            return (windowPoint / viewRect.size) * viewRect.size - viewRect.position;
        }
    }
}