using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.EditorUtils
{
    public class FlowchartWindowDrawGrid 
    {
        public virtual void Draw(FlowchartContext flowchartCtx, DrawGridContext gridCtx)
        {
            Flowchart currentFlowchart = flowchartCtx.Flowchart;
            Rect position = flowchartCtx.Position;
            Color gridLineColor = gridCtx.GridLineColor;
            float gridLineSpacingSize = gridCtx.GridLineSpacingSize;

            IList<float> xPositions, yPositions;

            GetPositions();
            void GetPositions()
            {
                xPositions = GridUtils.GetVerticalLinePositions(currentFlowchart.ScrollPos.x,
                    position.width / currentFlowchart.Zoom,
                    gridLineSpacingSize);
                yPositions = GridUtils.GetHorizontalLinePositions(currentFlowchart.ScrollPos.y,
                    position.height / currentFlowchart.Zoom,
                    gridLineSpacingSize);
            }

            DrawLines();
            void DrawLines()
            {
                Color prevHandlesColor = Handles.color;
                Handles.color = gridLineColor;
                float windowWidth = position.width / currentFlowchart.Zoom;
                float windowHeight = position.height / currentFlowchart.Zoom;

                DrawVerticalLines();
                void DrawVerticalLines()
                {
                    foreach (var elem in xPositions)
                        Handles.DrawLine(new Vector2(elem, 0), new Vector2(elem, windowHeight));
                }

                DrawHorizontalLines();
                void DrawHorizontalLines()
                {
                    foreach (var elem in yPositions)
                        Handles.DrawLine(new Vector2(0, elem), new Vector2(windowWidth, elem));
                }

                Handles.color = prevHandlesColor;
            }
        }
    }

    public class DrawGridContext
    {
        public virtual float GridLineSpacingSize { get; set; }
        public virtual Color GridLineColor { get; set; }
    }
}