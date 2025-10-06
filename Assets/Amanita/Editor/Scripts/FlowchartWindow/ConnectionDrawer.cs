using UnityEditor;
using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    public class ConnectionDrawer : IConnectionDrawer
    {
        public ConnectionDrawer(IConnectionGatherer gatherer)
        {
            _gatherer = gatherer;
        }

        protected readonly IConnectionGatherer _gatherer;

        public virtual void Draw(DrawBlockContext drawCtx, FlowchartContext fcContext)
        {
            var connections = _gatherer.GatherConnections(drawCtx);
            foreach (var c in connections)
                DrawRectConnection(c.FromRect, c.ToRect, c.Highlight);
        }

        protected virtual void DrawRectConnection(Rect fromRect, Rect toRect, bool highlight)
        {
            RegisterPointsOnSourceAndTargetBlocks();
            void RegisterPointsOnSourceAndTargetBlocks()
            {
                // Previous method made a lot of garbage, so now we reuse the same array
                // CGTespy's note: Seems that each block can have 4 points on which it can 
                // be connected to or from: left center, bottom center, top center, and right center.
                Vector2 leftCenter = new Vector2(fromRect.xMin, fromRect.center.y);
                pointsOnSourceRect[0] = leftCenter;

                Vector2 bottomCenter = new Vector2(fromRect.xMin + fromRect.width / 2, fromRect.yMin);
                pointsOnSourceRect[1] = bottomCenter;

                Vector2 topCenter = new Vector2(fromRect.xMin + fromRect.width / 2, fromRect.yMax);
                pointsOnSourceRect[2] = topCenter;

                Vector2 rightCenter = new Vector2(fromRect.xMax, fromRect.center.y);
                pointsOnSourceRect[3] = rightCenter;

                leftCenter = new Vector2(toRect.xMin, toRect.center.y);
                pointsOnTargetRect[0] = leftCenter;

                bottomCenter = new Vector2(toRect.xMin + toRect.width / 2, toRect.yMin);
                pointsOnTargetRect[1] = bottomCenter;

                topCenter = new Vector2(toRect.xMin + toRect.width / 2, toRect.yMax);
                pointsOnTargetRect[2] = topCenter;

                rightCenter = new Vector2(toRect.xMax, toRect.center.y);
                pointsOnTargetRect[3] = rightCenter;
            }

            Vector2 pointA = Vector2.zero;
            Vector2 pointB = Vector2.zero;
            float minDist = float.MaxValue;

            // Previous method compared every point to every point
            // We only check matching opposing mids
            for (int i = 0; i < closestAnchorPairs.Length; i++)
            {
                var a = pointsOnSourceRect[closestAnchorPairs[i].firstIndex];
                var b = pointsOnTargetRect[closestAnchorPairs[i].secondIndex];
                float currentDist = Vector2.Distance(a, b);
                if (currentDist < minDist)
                {
                    pointA = a;
                    pointB = b;
                    minDist = currentDist;
                }
            }

            SetHandlesColor();
            void SetHandlesColor()
            {
                Color color = connectionColor;
                if (highlight)
                {
                    color = Color.green;
                }

                Handles.color = color;
            }

            // Place control based on distance between points
            // Weight the min component more so things don't get overly curvy
            var diff = pointA - pointB;
            diff.x = Mathf.Abs(diff.x);
            diff.y = Mathf.Abs(diff.y);
            var min = Mathf.Min(diff.x, diff.y);
            var max = Mathf.Max(diff.x, diff.y);
            var mod = min * 0.75f + max * 0.25f;

            Vector2 directionA, directionB, controlA, controlB;
            DrawBezierCurveConnectingBlocks();
            void DrawBezierCurveConnectingBlocks()
            {
                directionA = (fromRect.center - pointA).normalized;
                directionB = (toRect.center - pointB).normalized;
                controlA = pointA - directionA * mod * 0.67f;
                controlB = pointB - directionB * mod * 0.67f;
                Handles.DrawBezier(pointA, pointB, controlA, controlB, Handles.color, null, 3f);
            }

            DrawArrowOnCurve();
            void DrawArrowOnCurve()
            {
                float arrowT = 0.7f;
                Vector2 midPoint = GetPointOnCurve(pointA, controlA, controlB, pointB, arrowT);
                Vector2 aheadPoint = GetPointOnCurve(pointA, controlA, controlB, pointB, arrowT + 0.1f);

                Vector2 travelDir = (aheadPoint - midPoint).normalized;
                // perpendicular (swap sign if it flips wrong)
                Vector2 perp = new Vector2(-travelDir.y, travelDir.x);

                bezierWorkspace[0] = midPoint;
                bezierWorkspace[1] = midPoint + travelDir * 10f + perp * 5f;
                bezierWorkspace[2] = midPoint + travelDir * 10f - perp * 5f;
                Handles.DrawAAConvexPolygon(bezierWorkspace);
            }

            var connectionPointA = pointA + directionA * 4f;
            var connectionRectA = new Rect(connectionPointA.x - 4f, connectionPointA.y - 4f, 8f, 8f);
            var connectionPointB = pointB + directionB * 4f;
            var connectionRectB = new Rect(connectionPointB.x - 4f, connectionPointB.y - 4f, 8f, 8f);

            GUI.DrawTexture(connectionRectA, ConnectionPointTex, ScaleMode.ScaleToFit);
            GUI.DrawTexture(connectionRectB, ConnectionPointTex, ScaleMode.ScaleToFit);

            Handles.color = Color.white; // Reset the col
        }

        protected Texture2D ConnectionPointTex { get { return AmanitaEditorResources.ConnectionPoint; } }
        protected static readonly Vector2[] pointsOnSourceRect = new Vector2[4];
        protected static readonly Vector2[] pointsOnTargetRect = new Vector2[4];
        protected readonly Color connectionColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);

        /// <summary>
        /// Samples a point along the cubic–Bezier curve between two node anchors.
        /// fromAnchor = exit point on source node
        /// fromControl = control handle on source side
        /// toControl = control handle on target side
        /// toAnchor = entry point on target node
        /// progress = normalized t parameter [0..1]
        /// </summary>
        public static Vector2 GetPointOnCurve(Vector2 fromAnchor, Vector2 fromControl,
            Vector2 toControl, Vector2 toAnchor, float progress)
        {
            float inverse = 1f - progress;
            float invSq = inverse * inverse;
            float progSq = progress * progress;
            float invCubed = invSq * inverse;
            float progCubed = progSq * progress;

            return invCubed * fromAnchor
                  + 3f * invSq * progress * fromControl
                  + 3f * inverse * progSq * toControl
                  + progCubed * toAnchor;
        }

        protected static readonly IndexPair[] closestAnchorPairs = new IndexPair[]
        {
            new IndexPair() { firstIndex=0,secondIndex=3 },
            new IndexPair() { firstIndex=3,secondIndex=0 },
            new IndexPair() { firstIndex=1,secondIndex=2 },
            new IndexPair() { firstIndex=2,secondIndex=1 },
        };

        //prevent alloc in DrawAAConvexPolygon
        static readonly Vector3[] bezierWorkspace = new Vector3[3];

    }

}