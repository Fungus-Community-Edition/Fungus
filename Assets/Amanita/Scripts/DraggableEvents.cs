using System;
using UnityEngine;

namespace AtMycelia.Amanita
{
    public static class DraggableEvents 
    {
        public static Action<Draggable2D> DragBeganTwoD = delegate { };
        public static Action<Draggable2D> DragEnteredTwoD = delegate { };
        public static Action<Draggable2D> DragExitedTwoD = delegate { };
    }

    public class DragStartedEvent
    {
        public Draggable2D DraggableObject;

        public DragStartedEvent(Draggable2D draggableObject)
        {
            DraggableObject = draggableObject;
        }
    }

    public class DragExitedEvent
    {
        public Draggable2D DraggableObject;
        public Collider2D TargetCollider;

        public DragExitedEvent(Draggable2D draggableObject, Collider2D targetCollider)
        {
            DraggableObject = draggableObject;
            TargetCollider = targetCollider;
        }
    }

    public class DragEnteredEvent
    {
        public Draggable2D DraggableObject;
        public Collider2D TargetCollider;

        public DragEnteredEvent(Draggable2D draggableObject, Collider2D targetCollider)
        {
            DraggableObject = draggableObject;
            TargetCollider = targetCollider;
        }
    }

    public class DragCompletedEvent
    {
        public Draggable2D DraggableObject;

        public DragCompletedEvent(Draggable2D draggableObject)
        {
            DraggableObject = draggableObject;
        }
    }

    public class DragCancelledEvent
    {
        public Draggable2D DraggableObject;

        public DragCancelledEvent(Draggable2D draggableObject)
        {
            DraggableObject = draggableObject;
        }
    }

    public class ObjectClickedEvent
    {
        public Clickable2D ClickableObject;
        public ObjectClickedEvent(Clickable2D clickableObject)
        {
            ClickableObject = clickableObject;
        }
    }
}