
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Listens for a specific input event and resets the scroll position of the flowchart to (0,0).
    /// </summary>
    public class ScrollPosResetter : IFlowchartWindowModule
    {
        public ScrollPosResetter(FlowchartContext fcContext)
        {
            _fcContext = fcContext;
        }

        private FlowchartContext _fcContext;

        public void Initialize(FlowchartWindowUitk owner)
        {
            this._owner = owner;
        }

        private FlowchartWindowUitk _owner;

        public void OnGUI()
        {
            Event cEvent = Event.current;
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