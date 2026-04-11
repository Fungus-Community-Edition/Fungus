using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public sealed class ConnectionDrawer : IDisposable
    {
        public ConnectionDrawer(IConnectionGatherer gatherer)
        {
            this.gatherer = gatherer ?? throw new ArgumentNullException(nameof(gatherer));
        }

        private readonly IConnectionGatherer gatherer;

        public void Dispose()
        {
            gatherer.Dispose();
        }

        public void Draw(Painter2D painter, DrawBlockContext drawCtx, FlowchartContext fcContext)
        {
            if (painter == null || drawCtx == null || fcContext == null)
            {
                return;
            }

            var connections = gatherer.GatherConnections(drawCtx);
            for (int i = 0; i < connections.Count; i++)
            {
                ConnectionInfo connection = connections[i];
                if (connection.FromBlock == null || connection.ToBlock == null)
                {
                    continue;
                }

                Rect fromRect = CalculateWindowRect(connection.FromBlock, fcContext.Flowchart);
                Rect toRect = CalculateWindowRect(connection.ToBlock, fcContext.Flowchart);
                DrawRectConnection(painter, fromRect, toRect, connection.Highlight);
            }
        }

        private static Rect CalculateWindowRect(Block block, Flowchart fc)
        {
            Rect modelRect = block._NodeRect;

            float zoom = 1f;
            Vector2 scrollPos = Vector2.zero;
            if (fc != null)
            {
                zoom = Mathf.Approximately(fc.Zoom, 0f) ? 1f : fc.Zoom;
                scrollPos = fc.ScrollPos;
            }

            modelRect.width *= zoom;
            modelRect.height *= zoom;
            modelRect.position = (modelRect.position + scrollPos) * zoom;
            return modelRect;
        }

        private void DrawRectConnection(Painter2D painter, Rect fromRect, Rect toRect, bool highlight)
        {
            RegisterPointsOnSourceAndTargetBlocks(fromRect, toRect);

            Vector2 sourceAnchor = Vector2.zero;
            Vector2 targetAnchor = Vector2.zero;
            float minDist = float.MaxValue;

            for (int i = 0; i < closestAnchorPairs.Length; i++)
            {
                Vector2 sourceAnchorCandidate = pointsOnSourceRect[closestAnchorPairs[i].firstIndex];
                Vector2 targetAnchorCandidate = pointsOnTargetRect[closestAnchorPairs[i].secondIndex];
                float currentDist = Vector2.Distance(sourceAnchorCandidate, targetAnchorCandidate);
                if (currentDist < minDist)
                {
                    sourceAnchor = sourceAnchorCandidate;
                    targetAnchor = targetAnchorCandidate;
                    minDist = currentDist;
                }
            }

            Color strokeColor = highlight ?
                Color.green :
                connectionColor;

            Vector2 diff = sourceAnchor - targetAnchor;
            diff.x = Mathf.Abs(diff.x);
            diff.y = Mathf.Abs(diff.y);
            float min = Mathf.Min(diff.x, diff.y);
            float max = Mathf.Max(diff.x, diff.y);
            float mod = min * 0.75f + max * 0.25f;

            Vector2 sourceDirection = (fromRect.center - sourceAnchor).normalized;
            Vector2 targetDirection = (toRect.center - targetAnchor).normalized;
            Vector2 sourceControl = sourceAnchor - sourceDirection * mod * 0.67f;
            Vector2 targetControl = targetAnchor - targetDirection * mod * 0.67f;

            var fChart = EditorSelectionTracker.ActiveFlowchart;
            float baseLineWidth = 3f;
            painter.lineWidth = fChart != null ?
                baseLineWidth * fChart.Zoom :
                3f;

            painter.strokeColor = strokeColor;
            painter.fillColor = strokeColor;

            painter.BeginPath();
            painter.MoveTo(sourceAnchor);
            painter.BezierCurveTo(sourceControl, targetControl, targetAnchor);
            painter.Stroke();

            DrawArrowOnCurve(painter, sourceAnchor, sourceControl, targetControl, targetAnchor);

            DrawConnectionPoint(painter, sourceAnchor + sourceDirection * 4f);
            DrawConnectionPoint(painter, targetAnchor + targetDirection * 4f);
        }

        private static void RegisterPointsOnSourceAndTargetBlocks(Rect fromRect, Rect toRect)
        {
            Vector2 leftCenter = new Vector2(fromRect.xMin, fromRect.center.y);
            pointsOnSourceRect[0] = leftCenter;

            Vector2 bottomCenter = new Vector2(fromRect.xMin + fromRect.width / 2f, fromRect.yMin);
            pointsOnSourceRect[1] = bottomCenter;

            Vector2 topCenter = new Vector2(fromRect.xMin + fromRect.width / 2f, fromRect.yMax);
            pointsOnSourceRect[2] = topCenter;

            Vector2 rightCenter = new Vector2(fromRect.xMax, fromRect.center.y);
            pointsOnSourceRect[3] = rightCenter;

            leftCenter = new Vector2(toRect.xMin, toRect.center.y);
            pointsOnTargetRect[0] = leftCenter;

            bottomCenter = new Vector2(toRect.xMin + toRect.width / 2f, toRect.yMin);
            pointsOnTargetRect[1] = bottomCenter;

            topCenter = new Vector2(toRect.xMin + toRect.width / 2f, toRect.yMax);
            pointsOnTargetRect[2] = topCenter;

            rightCenter = new Vector2(toRect.xMax, toRect.center.y);
            pointsOnTargetRect[3] = rightCenter;
        }

        private static void DrawArrowOnCurve(Painter2D painter, Vector2 startAnchor, Vector2 startControl,
            Vector2 endControl, Vector2 endAnchor)
        {
            float arrowT = 0.7f;
            Vector2 midPoint = GetPointOnCurve(startAnchor, startControl, endControl, endAnchor, arrowT);
            Vector2 aheadPoint = GetPointOnCurve(startAnchor, startControl, endControl, endAnchor, arrowT + 0.1f);

            Vector2 travelDir = (midPoint - aheadPoint).normalized;
            Vector2 perp = new Vector2(-travelDir.y, travelDir.x);

            float zoom = 1f;
            var fChart = EditorSelectionTracker.ActiveFlowchart;
            if (fChart != null)
            {
                zoom = Mathf.Approximately(fChart.Zoom, 0f) ?
                    1f :
                    fChart.Zoom;
            }

            float arrowLength = 10f * zoom;
            float arrowWidth = 5f * zoom;

            Vector2 tip = midPoint;
            Vector2 left = midPoint + travelDir * arrowLength + perp * arrowWidth;
            Vector2 right = midPoint + travelDir * arrowLength - perp * arrowWidth;

            painter.BeginPath();
            painter.MoveTo(tip);
            painter.LineTo(left);
            painter.LineTo(right);
            painter.ClosePath();
            painter.Fill();
        }

        private static void DrawConnectionPoint(Painter2D painter, Vector2 center)
        {
            var fChart = EditorSelectionTracker.ActiveFlowchart;
            float radius = ConnectionPointRadius;
            if (fChart != null)
            {
                radius *= fChart.Zoom;
            }
            Color prevColor = painter.fillColor;
            painter.fillColor = painter.strokeColor = Color.green;
            painter.BeginPath();
            painter.Arc(center, radius, 0f, 360f);
            painter.Fill();
            painter.Stroke();
            painter.fillColor = painter.strokeColor = prevColor;
        }

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

        private const float ConnectionPointRadius = 4f;

        private static readonly Vector2[] pointsOnSourceRect = new Vector2[4];
        private static readonly Vector2[] pointsOnTargetRect = new Vector2[4];

        private static readonly IndexPair[] closestAnchorPairs = new IndexPair[]
        {
            new IndexPair(0, 3),
            new IndexPair(3, 0),
            new IndexPair(1, 2),
            new IndexPair(2, 1)
        };

        private static readonly Color connectionColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);
    }
}