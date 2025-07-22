using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    public class ConnectionRenderer
    {
        public ConnectionRenderer(IConnectionDrawer connectionDrawer)
        {
            _drawer = connectionDrawer;
        }
        protected readonly IConnectionDrawer _drawer;

        public virtual void Render(DrawBlockContext drawCtx, FlowchartContext fcContext)
        {
            _drawer.Draw(drawCtx, fcContext);

        }

    }

    public interface IConnectionDrawer
    {
        void Draw(DrawBlockContext drawCtx, FlowchartContext fcContext);
    }

    public class ConnectionDrawer : IConnectionDrawer
    {

        protected Texture2D ConnectionPointTex { get { return AmanitaEditorResources.ConnectionPoint;  } }

        public virtual void Draw(DrawBlockContext drawCtx, FlowchartContext fcContext)
        {
            Flowchart fc = fcContext.Flowchart;
            IList<Block> validBlocks = fcContext.AllBlocks.Where((elem) => elem != null).ToList();

            for (int i = 0; i < validBlocks.Count; i++)
            {
                Block currentBlock = validBlocks[i];
                bool blockIsSelected = fc.SelectedBlock == currentBlock;

                Rect scriptViewRect = drawCtx.ViewRect;

                IList<Command> validCommands = currentBlock.CommandList.Where((elem) => elem != null).ToList();
                var commandList = currentBlock.CommandList;
                foreach (var commandEl in validCommands)
                {
                    var selectedCommands = fc.SelectedCommands;
                    bool commandIsSelected = fc.SelectedCommands.Contains(commandEl);
                    bool highlight = commandEl.IsExecuting || (blockIsSelected && commandIsSelected);

                    connectedBlocks.Clear();
                    commandEl.GetConnectedBlocks(ref connectedBlocks);

                    // We don't want to draw connections to blocks that are:
                    // - null
                    // - the same block as the one we want to draw connections from
                    // - outside the current Block's Flowchart
                    IList<Block> toDrawConnectionsFrom = (from elem in connectedBlocks
                                                          where elem != null
                                                          where elem != currentBlock
                                                          where elem.GetFlowchart().Equals(fc)
                                                          select elem).ToList();

                    foreach (var connectedBlockElem in toDrawConnectionsFrom)
                    {
                        Rect fromRect = new Rect(currentBlock._NodeRect);
                        fromRect.x += fc.ScrollPos.x;
                        fromRect.y += fc.ScrollPos.y;

                        Rect toRect = new Rect(connectedBlockElem._NodeRect);
                        toRect.x += fc.ScrollPos.x;
                        toRect.y += fc.ScrollPos.y;

                        Rect boundRect = new Rect();
                        boundRect.xMin = Mathf.Min(fromRect.xMin, toRect.xMin);
                        boundRect.xMax = Mathf.Max(fromRect.xMax, toRect.xMax);
                        boundRect.yMin = Mathf.Min(fromRect.yMin, toRect.yMin);
                        boundRect.yMax = Mathf.Max(fromRect.yMax, toRect.yMax);

                        if (boundRect.Overlaps(scriptViewRect))
                            DrawRectConnection(fromRect, toRect, highlight);
                    }
                }
            }
        }

        // Keeping things all in one list for performance reasons
        protected List<Block> connectedBlocks = new List<Block>();

        protected virtual void DrawRectConnection(Rect fromRect, Rect toRect, bool highlight)
        {
            // Previous method made a lot of garbage, so now we reuse the same array
            pointsA[0] = new Vector2(fromRect.xMin, fromRect.center.y);
            pointsA[1] = new Vector2(fromRect.xMin + fromRect.width / 2, fromRect.yMin);
            pointsA[2] = new Vector2(fromRect.xMin + fromRect.width / 2, fromRect.yMax);
            pointsA[3] = new Vector2(fromRect.xMax, fromRect.center.y);

            pointsB[0] = new Vector2(toRect.xMin, toRect.center.y);
            pointsB[1] = new Vector2(toRect.xMin + toRect.width / 2, toRect.yMin);
            pointsB[2] = new Vector2(toRect.xMin + toRect.width / 2, toRect.yMax);
            pointsB[3] = new Vector2(toRect.xMax, toRect.center.y);

            Vector2 pointA = Vector2.zero;
            Vector2 pointB = Vector2.zero;
            float minDist = float.MaxValue;

            //previous method compared every point to every point
            //  we only check matching opposing mids
            for (int i = 0; i < closestCornerIndexPairs.Length; i++)
            {
                var a = pointsA[closestCornerIndexPairs[i].a];
                var b = pointsB[closestCornerIndexPairs[i].b];
                float d = Vector2.Distance(a, b);
                if (d < minDist)
                {
                    pointA = a;
                    pointB = b;
                    minDist = d;
                }
            }

            Color color = connectionColor;
            if (highlight)
            {
                color = Color.green;
            }

            Handles.color = color;

            // Place control based on distance between points
            // Weight the min component more so things don't get overly curvy
            var diff = pointA - pointB;
            diff.x = Mathf.Abs(diff.x);
            diff.y = Mathf.Abs(diff.y);
            var min = Mathf.Min(diff.x, diff.y);
            var max = Mathf.Max(diff.x, diff.y);
            var mod = min * 0.75f + max * 0.25f;

            // Draw bezier curve connecting blocks
            var directionA = (fromRect.center - pointA).normalized;
            var directionB = (toRect.center - pointB).normalized;
            var controlA = pointA - directionA * mod * 0.67f;
            var controlB = pointB - directionB * mod * 0.67f;
            Handles.DrawBezier(pointA, pointB, controlA, controlB, color, null, 3f);

            // Draw arrow on curve
            var point = GetPointOnCurve(pointA, controlA, pointB, controlB, 0.7f);
            var direction = (GetPointOnCurve(pointA, controlA, pointB, controlB, 0.6f) - point).normalized;
            var perp = new Vector2(direction.y, -direction.x);
            //reuse same array to avoid the auto alloced one in DrawAAConvexPolygon
            beizerWorkSpace[0] = point;
            beizerWorkSpace[1] = point + direction * 10 + perp * 5;
            beizerWorkSpace[2] = point + direction * 10 - perp * 5;
            Handles.DrawAAConvexPolygon(beizerWorkSpace);

            var connectionPointA = pointA + directionA * 4f;
            var connectionRectA = new Rect(connectionPointA.x - 4f, connectionPointA.y - 4f, 8f, 8f);
            var connectionPointB = pointB + directionB * 4f;
            var connectionRectB = new Rect(connectionPointB.x - 4f, connectionPointB.y - 4f, 8f, 8f);

            GUI.DrawTexture(connectionRectA, ConnectionPointTex, ScaleMode.ScaleToFit);
            GUI.DrawTexture(connectionRectB, ConnectionPointTex, ScaleMode.ScaleToFit);

            Handles.color = Color.white;
        }

        protected static readonly Vector2[] pointsA = new Vector2[4];
        protected static readonly Vector2[] pointsB = new Vector2[4];
        protected readonly Color connectionColor = new Color(0.65f, 0.65f, 0.65f, 1.0f);

        protected static Vector2 GetPointOnCurve(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {
            float rt = 1 - t;
            float rtt = rt * t;
            return rt * rt * rt * s + 3 * rt * rtt * st + 3 * rtt * t * et + t * t * t * e;
        }

        //we only connect mids on sides to matching opposing middle side on other block
        protected struct IndexPair { public int a, b; public IndexPair(int a, int b) { this.a = a; this.b = b; } }
        static readonly IndexPair[] closestCornerIndexPairs = new IndexPair[]
        {
            new IndexPair() { a=0,b=3 },
            new IndexPair() { a=3,b=0 },
            new IndexPair() { a=1,b=2 },
            new IndexPair() { a=2,b=1 },
        };

        //prevent alloc in DrawAAConvexPolygon
        static readonly Vector3[] beizerWorkSpace = new Vector3[3];

        protected Texture2D connectionPointTexture;

    }


}