using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    public class FcWindowExecutionVisualizer : IFcWindowComponent
    {
        public void Initialize(FlowchartWindow window)
        {
            _window = window;
            _execTracker = new FlowchartWindow.ExecutingBlocks();
        }

        protected FlowchartWindow _window;
        protected FlowchartWindow.ExecutingBlocks _execTracker;

        public void OnEditorUpdate()
        {
            if (Application.isPlaying)
            {
                _execTracker.ProcessAllBlocks(_window.blocks);
                if (_execTracker.isChangeDetected || _execTracker.IsAnimFadeoutNeed())
                    _window.Repaint();
            }
            else
            {
                _execTracker.ClearAll();
            }
        }

        public void OnToolbarGUI() { }

        public void OnCanvasGUI(DrawBlockContext drawCtx, FlowchartContext fcCtx)
        {
            if (Event.current.type != EventType.Repaint || !Application.isPlaying)
                return;

            // same “world → screen” rect you used for zoom
            Rect viewRect = _window.CalcFlowchartWindowViewRect();
            var curTime = Time.realtimeSinceStartup;
            var style = new GUIStyle();

            // iterate all blocks in the context
            foreach (var block in fcCtx.AllBlocks)
            {
                float alpha = (block.ExecutingIconTimer - curTime)
                            / AmanitaConstants.ExecutingIconFadeTime;

                DrawExecutingBlockIcon(block, viewRect, alpha, style);
            }
        }

        public void OnInspectorGUI() { }

        // —— helper pulled verbatim from your FlowchartWindow ——
        void DrawExecutingBlockIcon(
            Block executingBlock,
            Rect viewRect,
            float alpha,
            GUIStyle style)
        {
            if (alpha <= 0f)
                return;

            Rect rect = new Rect(executingBlock._NodeRect);
            rect.x += _window.Flowchart.ScrollPos.x - 37;
            rect.y += _window.Flowchart.ScrollPos.y + 3;
            rect.width = 34;
            rect.height = 34;

            if (viewRect.Overlaps(rect))
            {
                GUI.color = new Color(1f, 1f, 1f, alpha);
                if (GUI.Button(rect, AmanitaEditorResources.PlayBig, style))
                {
                    _window.SelectBlock(executingBlock);
                }
                GUI.color = Color.white;
            }
        }

        public void OnInspectorUpdate()
        {
            
        }
    }
}