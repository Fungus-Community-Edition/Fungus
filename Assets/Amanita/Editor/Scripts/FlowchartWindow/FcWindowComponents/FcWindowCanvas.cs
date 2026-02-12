using UnityEngine;
using Amanita.EditorUtils;
using System.Collections.Generic;

namespace Amanita.VScripting.EditorUtils
{
    public class FcWindowCanvas : IFcWindowComponent
    {
        public virtual void Dispose()
        {
            ToggleSubs(false);
            _window = null;
            _gridRenderer.Dispose();
            _blockRenderer.Dispose();
            _flowchartCtx.Dispose();
            _drawGridCtx.Dispose();
            _drawBlockCtx.Dispose();
            _connectionRenderer.Dispose();
        }

        public virtual void Initialize(IFlowchartViewHost window)
        {
            _window = window;
            _gridRenderer = new GridRenderer(new HandlesLineDrawer());
            _blockRenderer = new BlockRenderer(new DefaultBlockDrawer(), new BlockGraphicsGenerator());
            _connectionRenderer = new ConnectionRenderer(new ConnectionDrawer(new ConnectionGatherer()));

            // Share contexts with window
            _drawGridCtx = window.DrawGridCtx;
            _drawBlockCtx = window.DrawBlockCtx;
            _flowchartCtx = window.FlowchartCtx;
            ToggleSubs(true);
        }

        protected IFlowchartViewHost _window;
        protected GridRenderer _gridRenderer;
        protected BlockRenderer _blockRenderer;
        protected ConnectionRenderer _connectionRenderer;
        protected DrawGridContext _drawGridCtx;
        protected DrawBlockContext _drawBlockCtx;
        protected FlowchartContext _flowchartCtx;

        void ToggleSubs(bool on)
        {
            if (on)
            {
                BlockSignals.BlockCreated += OnBlockCreated;
                BlockSignals.PreBlockDelete += OnPreBlockDelete;
                BlockSignals.PreMultiBlockDelete += OnPreMultiBlockDelete;
            }
            else
            {
                BlockSignals.BlockCreated -= OnBlockCreated;
            }
        }

        private void OnPreMultiBlockDelete(IList<Block> list)
        {
            DrawBlocksAndConnections();
        }

        private void OnPreBlockDelete(Block block)
        {
            DrawBlocksAndConnections();
        }

        private void OnBlockCreated(Block block)
        {
            DrawBlocksAndConnections();
        }

        void DrawBlocksAndConnections()
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                _window?.Repaint();
                return;
            }

            _blockRenderer.Render(_drawBlockCtx);
            _connectionRenderer.Render(_drawBlockCtx, _flowchartCtx);
        }

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
                DrawBlocksAndConnections();
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