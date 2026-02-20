using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils.FcWindow
{
    public class HitDetectionHandlerUitk : IFlowchartWindowModule, ILeftMouseDownResponder
    {
        public int Priority { get; set; } = 0;
        public void Initialize(FlowchartWindowUitk window)
        {
            if (window == null)
            {
                throw new System.ArgumentNullException(nameof(window));
            }
            owner = window;
            isDisposed = false;
            ToggleSubs(true);
        }
        private FlowchartWindowUitk owner;
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
            ResetSelectionBox();
            Block blockHit = TopmostBlockOverlapping(eventInfo.FlowchartPosition);
            BlockHitInLastMouseDown = blockHit;
        }

        private FlowchartContext FcContext => owner.FcContext;
        private void ResetSelectionBox()
        {
            FcContext.Interaction.ResetSelectionBox();
        }

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