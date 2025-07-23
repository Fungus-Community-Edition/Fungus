using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    public class FcWindowCanvas : IFcWindowComponent
    {
        // These were originally fields on FlowchartWindow
        private GridRenderer _gridRenderer;
        private BlockRenderer _blockRenderer;
        private ConnectionRenderer _connectionRenderer;
        private DrawGridContext _drawGridCtx;
        private DrawBlockContext _drawBlockCtx;
        private FlowchartContext _flowchartCtx;
        private FlowchartWindow _window;

        public void Initialize(FlowchartWindow window)
        {
            _window = window;
            _gridRenderer = new GridRenderer(new HandlesLineDrawer());
            _blockRenderer = new BlockRenderer(new DefaultBlockDrawer(), new BlockGraphicsGenerator());
            _connectionRenderer = new ConnectionRenderer(new ConnectionDrawer(new ConnectionGatherer()));

            // Share contexts with window
            _drawGridCtx = window.drawGridCtx;
            _drawBlockCtx = window._drawBlockContext;
            _flowchartCtx = window.flowchartCtx;
        }

        public void OnEditorUpdate()
        {
            // nothing to do per-frame on canvas
        }

        public void OnToolbarGUI()
        {
            // no toolbar elements here
        }

        public void OnCanvasGUI(DrawBlockContext drawCtx, FlowchartContext fcCtx)
        {
            DrawBackgroundAndGrid();
            void DrawBackgroundAndGrid()
            {
                if (Event.current.type == EventType.Repaint)
                {
                    UnityEditor.Graphs.Styles.graphBackground.Draw(
                    new Rect(0, 17, _window.position.width, _window.position.height - 17),
                    false, false, false, false
                    );
                    _drawGridCtx.GridLineColor = _window.gridLineColor;
                    _drawGridCtx.GridLineSpacingSize = 120;
                    _gridRenderer.Draw(_flowchartCtx, _drawGridCtx);
                }
            }

            Rect scriptViewRect = _window.CalcFlowchartWindowViewRect();

            EditorZoomArea.Begin(_window.Flowchart.Zoom, scriptViewRect);
            // Update contexts
            _flowchartCtx.Flowchart = _window.Flowchart;
            _drawBlockCtx.ViewRect = scriptViewRect;

            if (Event.current.type == EventType.Repaint)
            {
                // Draw blocks & connections
                _blockRenderer.Render(_drawBlockCtx);
                _connectionRenderer.Render(_drawBlockCtx, _flowchartCtx);
            }


            EditorZoomArea.End();
        }

        public void OnInspectorGUI()
        {
            // nothing in the inspector area
        }

        public void OnInspectorUpdate()
        {
        }
    }
}