
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Listens for a specific input event and resets the scroll position of the flowchart to (0,0).
    /// Basically QOL for users who want to quickly reset their view.
    /// </summary>
    public class ScrollPosResetter : IFlowchartWindowModule
    {
        public int Priority { get; set; } = 0;
        public ScrollPosResetter(FlowchartContext fcContext)
        {
            _fcContext = fcContext;
        }

        private FlowchartContext _fcContext;

        public void Initialize(FlowchartWindow owner)
        {
            this._owner = owner;
        }

        private FlowchartWindow _owner;

        public void OnGUI(Event cEvent)
        {
            bool pressedShift = cEvent.shift;
            bool pressedRKey = cEvent.keyCode == KeyCode.R && (cEvent.type == EventType.KeyDown);
            if (pressedShift && pressedRKey)
            {
                Debug.Log("Resetting scroll position to (0,0).");
                ResetScrollPos(_fcContext.Flowchart);
            }
        }

        private void ResetScrollPos(Flowchart flowchart)
        {
            if (flowchart == null)
            {
                Debug.LogWarning("Cannot reset scroll position: Flowchart is null.");
                return;
            }
            flowchart.ScrollPos = Vector2.zero;
            FlowchartWindowSignals.WindowPanned.Invoke();
        }

        public void Dispose()
        {
            _owner = null;
            _fcContext = null;
        }


    }

}