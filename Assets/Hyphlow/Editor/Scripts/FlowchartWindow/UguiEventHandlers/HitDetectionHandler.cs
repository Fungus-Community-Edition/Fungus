using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Handles hit detection for mouse clicks in the FlowchartWindow, determining which 
    /// Block (if any) was hit and storing it in the Interaction context for use by other modules.
    /// </summary>
    public class HitDetector : IFlowchartWindowModule, ILeftMouseDownResponder
    {
        public int Priority { get; set; } = 0;
        public void Initialize(FlowchartWindow window)
        {
            if (window == null)
            {
                throw new System.ArgumentNullException(nameof(window));
            }
            owner = window;
            isDisposed = false;
            ToggleSubs(true);
        }
        private FlowchartWindow owner;
        private bool isDisposed;

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                FlowchartWindowSignals.LeftMouseDown += OnMouseDown;
                FlowchartWindowSignals.RightMouseDown += OnMouseDown;
            }
            else
            {
                FlowchartWindowSignals.LeftMouseDown -= OnMouseDown;
                FlowchartWindowSignals.RightMouseDown -= OnMouseDown;
            }
        }

        private void OnMouseDown(PointerEventInfo eventInfo)
        {
            Block blockHit = TopmostBlockOverlapping(eventInfo.PanelPosition);
            owner.FcContext.Interaction.BlockHitInLastMouseDown = blockHit;
            BlockHitInLastMouseDown = blockHit;

            if (blockHit != null)
            {
                //Debug.Log($"Hit block: {blockHit.BlockName}");
            }
        }

        private FlowchartContext FcContext => owner.FcContext;

        private Block TopmostBlockOverlapping(Vector2 mousePos)
        {
            return BlockHitTester.FindTopmostBlock(mousePos);
        }

        private Block BlockHitInLastMouseDown
        {
            set => FcContext.Interaction.BlockHitInLastMouseDown = value;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            ToggleSubs(false);
            isDisposed = true;
        }

        public void OnLeftMouseDown(PointerEventInfo info)
        {
            OnMouseDown(info);
        }
    }
}