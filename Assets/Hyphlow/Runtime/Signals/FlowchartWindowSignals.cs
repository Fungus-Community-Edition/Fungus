using System;
using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Struct that encapsulates information about pointer events in the flowchart window, such as mouse clicks 
    /// and drags. This includes the position of the pointer in both flowchart and panel coordinates, as well 
    /// as the delta movement since the last event.
    /// 
    /// We need this to make it easier to respond to drag events, given the differences between panel space in
    /// the Flowchart Window as well as the coordinate space of the flowchart itself. Without this convenience,
    /// it can be harder to properly do things like tell when a Block or empty space is clicked.
    /// </summary>
    public struct PointerEventInfo
    {
        public PointerEventInfo(Vector2 flowchartPosition, Vector2 panelPosition, Vector2 flowchartDelta, Vector2 panelDelta)
        {
            FlowchartPosition = flowchartPosition;
            PanelPosition = panelPosition;
            FlowchartDelta = flowchartDelta;
            PanelDelta = panelDelta;
        }

        public Vector2 FlowchartPosition { get; set;  }
        public Vector2 PanelPosition { get; set;  }
        public Vector2 FlowchartDelta { get; set;  }
        public Vector2 PanelDelta { get; set;  }
    }

    public static class FlowchartWindowSignals
    {

        /// <summary>
        /// Invoked when the user left-clicks inside the flowchart window just once.
        /// </summary>
        public static Action<PointerEventInfo> LeftMouseDown = delegate { };
        public static Action<PointerEventInfo> RightMouseDown = delegate { };
        public static Action<PointerEventInfo, Event> RightMouseUp = delegate { };
        public static Action<PointerEventInfo> LeftClicked = delegate { };

        /// <summary>
        /// Invoked when the user double-left-clicks inside the flowchart window.
        /// </summary>
        public static Action<PointerEventInfo> DoubleClicked = delegate { };

        public static Action ScrollWheelMoved = delegate { };

        /// <summary>
        /// Invoked when the user drags the scroll wheel inside the flowchart window. The argument
        /// passed is the mouse movement since the last event.
        /// </summary>
        public static Action<Vector2> ScrollWheelDragged = delegate { };

        public static Action<PointerEventInfo, Event> EmptySpaceLeftMouseDown = delegate { };
        public static Action<PointerEventInfo, Event> EmptySpaceLeftMouseUp = delegate { };
        public static Action<PointerEventInfo, Event> LeftMouseUp = delegate { };

        public static Action<PointerEventInfo, Event> EmptySpaceRightMouseDown = delegate { };
        public static Action<PointerEventInfo, Event> EmptySpaceRightMouseUp = delegate { };

        public static Action<PointerEventInfo> EmptySpaceLeftClicked = delegate { };
        public static Action<PointerEventInfo> EmptySpaceRightClicked = delegate { };

        /// <summary>
        /// Invoked when it's time for the Flowchart Window to focus on a different Flowchart.
        /// The first argument is the  old flowchart, and the second argument is the new flowchart. 
        /// If there was no previous flowchart, the first argument will be null. If there is no new
        /// flowchart, the second argument will be null. This is the signal that submodules of the 
        /// flowchart window should listen to in order to know when to respond
        /// to Flowchart-selection changes.
        /// </summary>
        public static Action<Flowchart, Flowchart> ChangedFlowchart = delegate { };
        public static Action WindowPanned = delegate { };

        public static Action<PointerEventInfo, Event> LeftMouseDragStarted = delegate { };
        public static Action<PointerEventInfo, Event> LeftMouseDragged = delegate { };
        public static Action<PointerEventInfo, Event> LeftMouseDragEnded = delegate { };

        public static Action<PointerEventInfo, Event> RightMouseDragStarted = delegate { };
        public static Action<PointerEventInfo, Event> RightMouseDragged = delegate { };
        public static Action<PointerEventInfo, Event> RightMouseDragEnded = delegate { };

        public static Action<float> ZoomChanged = delegate { };
    }

    // Interfaces for subscribing to flowchart window signals
    public interface ILeftMouseDownResponder
    {
        void OnLeftMouseDown(PointerEventInfo info);
    }

    public interface ILeftClickResponder
    {
        void OnLeftClick(PointerEventInfo info);
    }

    public interface IRightClickResponder
    {
        void OnRightClick(PointerEventInfo info);
    }

    public interface IDoubleClickResponder
    {
        void OnDoubleClick(PointerEventInfo info);
    }

    public interface IScrollWheelMoveResponder
    {
        void OnScrollWheelMoved();
    }

    public interface IScrollWheelDragResponder
    {
        void OnScrollWheelDragged(Vector2 direction);
    }

    public interface IEmptySpaceLeftMouseDownResponder
    {
        void OnEmptySpaceLeftMouseDown(PointerEventInfo info, Event evt);
    }

    public interface ILeftMouseUpResponder
    {
        void OnLeftMouseUp(PointerEventInfo info, Event evt);
    }

    public interface IEmptySpaceLeftMouseUpResponder
    {
        void OnEmptySpaceLeftMouseUp(PointerEventInfo info, Event evt);
    }

    public interface IEmptySpaceLeftClickResponder
    {
        void OnEmptySpaceLeftClicked(PointerEventInfo info);
    }

    public interface IFlowchartChangeResponder
    {
        void OnFlowchartChanged(Flowchart oldFc, Flowchart newFc);
    }

    public interface IWindowPanResponder
    {
        void OnWindowPanned();
    }

    public interface ILeftMouseDragStartResponder
    {
        void OnLeftMouseDragStarted(PointerEventInfo info, Event evt);
    }

    public interface ILeftMouseDragResponder
    {
        void OnLeftMouseDragged(PointerEventInfo info, Event evt);
    }

    public interface ILeftMouseDragEndResponder
    {
        void OnLeftMouseDragEnded(PointerEventInfo info, Event evt);
    }

    public interface IRightMouseDragStartResponder
    {
        void OnRightMouseDragStarted(PointerEventInfo info, Event evt);
    }

    public interface IRightMouseDragResponder
    {
        void OnRightMouseDragged(PointerEventInfo info, Event evt);
    }

    public interface IRightMouseDragEndResponder
    {
        void OnRightMouseDragEnded(PointerEventInfo info, Event evt);
    }

}