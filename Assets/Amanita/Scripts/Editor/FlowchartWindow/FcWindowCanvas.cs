using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    public class FcWindowCanvas : IFcWindowComponent
    {
        public virtual void Dispose()
        {
            _window = null;
            _gridRenderer.Dispose();
            _blockRenderer.Dispose();
            _flowchartCtx.Dispose();
            _drawGridCtx.Dispose();
            _drawBlockCtx.Dispose();
            _connectionRenderer.Dispose();
        }

        public virtual void Initialize(IFlowchartHost window)
        {
            _window = window;
            _gridRenderer = new GridRenderer(new HandlesLineDrawer());
            _blockRenderer = new BlockRenderer(new DefaultBlockDrawer(), new BlockGraphicsGenerator());
            _connectionRenderer = new ConnectionRenderer(new ConnectionDrawer(new ConnectionGatherer()));

            // Share contexts with window
            _drawGridCtx = window.DrawGridCtx;
            _drawBlockCtx = window.DrawBlockCtx;
            _flowchartCtx = window.FlowchartCtx;
        }

        protected IFlowchartHost _window;
        protected GridRenderer _gridRenderer;
        protected BlockRenderer _blockRenderer;
        protected ConnectionRenderer _connectionRenderer;
        protected DrawGridContext _drawGridCtx;
        protected DrawBlockContext _drawBlockCtx;
        protected FlowchartContext _flowchartCtx;

        public virtual void OnEditorUpdate()
        {
            // nothing to do per-frame on canvas
        }

        public virtual void OnToolbarGUI()
        {
            // no toolbar elements here
        }

        public virtual void OnGUI(DrawBlockContext drawCtx, FlowchartContext fcCtx)
        {
            DrawBackgroundAndGrid();
            void DrawBackgroundAndGrid()
            {
                if (Event.current.type == EventType.Repaint)
                {
                    Rect newPos = new Rect(0, 17, _window.Position.width, _window.Position.height - 17);
                    UnityEditor.Graphs.Styles.graphBackground.Draw(newPos, isHover: false, isActive: false,
                        on: false, hasKeyboardFocus: false);

                    _drawGridCtx.GridLineColor = _window.GridLineColor;
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

        public virtual void OnInspectorGUI()
        {
            // nothing in the inspector area
        }

        public virtual void OnInspectorUpdate()
        {
        }
    }
}