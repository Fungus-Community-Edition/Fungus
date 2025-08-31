using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// For drawing the UI elements letting you know that a Block or Command
    /// in a Flowchart is executing.
    /// </summary>
    public class FcWindowExecutionVisualizer : IFcWindowComponent
    {
        public virtual void Dispose()
        {
            _window = null;
            _execTracker = null;
            _iconStyle = null;
            viewRect = default;
        }

        public virtual void Initialize(IFlowchartHost window)
        {
            _window = window;
            _execTracker = new FlowchartWindow.ExecutingBlocks();
            _iconStyle = new GUIStyle();
        }

        protected IFlowchartHost _window;
        protected FlowchartWindow.ExecutingBlocks _execTracker;
        protected GUIStyle _iconStyle;

        public virtual void OnEditorUpdate()
        {
            if (Application.isPlaying)
            {
                _execTracker.ProcessAllBlocks(_window.Blocks);
                if (_execTracker.isChangeDetected || _execTracker.IsAnimFadeoutNeed())
                    _window.Repaint();
            }
            else
            {
                _execTracker.ClearAll();
            }
        }

        public virtual void OnToolbarGUI() { }

        public virtual void OnGUI(DrawBlockContext drawCtx, FlowchartContext flowchartContext)
        {
            if (Event.current.type != EventType.Repaint || !Application.isPlaying)
                return;

            // same “world ? screen” rect you used for zoom
            viewRect = _window.CalcFlowchartWindowViewRect();
            var curTime = Time.realtimeSinceStartup;

            foreach (var block in flowchartContext.AllBlocks)
            {
                float alpha = (block.ExecutingIconTimer - curTime)
                            / AmanitaConstants.ExecutingIconFadeTime;

                DrawExecutingBlockIcon(block, alpha);
            }
        }

        protected Rect viewRect;

        public virtual void OnInspectorGUI() { }

        protected virtual void DrawExecutingBlockIcon(Block executingBlock,
            float alpha)
        {
            if (alpha <= 0f)
                return;

            Rect rect = new Rect(executingBlock._NodeRect);
            float toTheRightEdge = _window.Flowchart.ScrollPos.x - (blockWidth + horizPadding);
            rect.x += toTheRightEdge;
            rect.y += _window.Flowchart.ScrollPos.y + 3;
            rect.width = rect.height = blockWidth; // We want it to be a square

            bool visibleInFcWindow = viewRect.Overlaps(rect);
            if (visibleInFcWindow)
            {
                GUI.color = new Color(1f, 1f, 1f, alpha);
                if (GUI.Button(rect, AmanitaEditorResources.PlayBig, _iconStyle))
                {
                    _window.SelectBlock(executingBlock);
                }
                GUI.color = Color.white;
            }
        }

        protected static readonly float blockWidth = 34, horizPadding = 3;

        public virtual void OnInspectorUpdate()
        {
            
        }
    }
}
