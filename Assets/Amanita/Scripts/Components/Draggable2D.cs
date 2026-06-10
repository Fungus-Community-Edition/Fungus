using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using AtMycelia.HyphaTween;

namespace AtMycelia.Amanita
{
    /// <summary>
    /// Probe event raised by Draggable2D at drag end to let listeners decide if this drag
    /// counts as completed.
    /// </summary>
    public sealed class DragEndProbeEvent
    {
        public DragEndProbeEvent(Draggable2D draggableObject, IEnumerable<Collider2D> overlappingTargets)
        {
            DraggableObject = draggableObject;
            _overlappingTargets = overlappingTargets != null
                ? new List<Collider2D>(overlappingTargets)
                : new List<Collider2D>();
        }

        public Draggable2D DraggableObject { get; }

        private readonly List<Collider2D> _overlappingTargets;

        public IReadOnlyList<Collider2D> OverlappingTargets
        {
            get { return _overlappingTargets; }
        }

        public bool IsCompleted { get; private set; }

        public Collider2D TargetCollider { get; private set; }

        public void MarkCompleted(Collider2D targetCollider)
        {
            IsCompleted = true;
            if (TargetCollider == null)
            {
                TargetCollider = targetCollider;
            }
        }
    }

    /// <summary>
    /// Detects drag and drop interactions on a Game Object, and sends events to all 
    /// Flowchart event handlers in the scene.
    /// 
    /// The Game Object must have Collider2D & RigidBody components attached. 
    /// The Collider2D must have the Is Trigger property set to true.
    /// 
    /// The RigidBody would typically have the Is Kinematic property set to true, 
    /// unless you want the object to move around using physics.
    /// 
    /// Use in conjunction with the Drag Started, Drag Completed, Drag Cancelled, 
    /// Drag Entered & Drag Exited event handlers.
    /// </summary>
    public class Draggable2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("Is object dragging enabled")]
        [FormerlySerializedAs("dragEnabled")]
        [SerializeField] protected bool _dragEnabled = true;

        [Tooltip("Move object back to its starting position when drag is cancelled")]
        [FormerlySerializedAs("returnOnCancelled")]
        [SerializeField] protected bool _returnOnCancelled = true;

        [Tooltip("Move object back to its starting position when drag is completed")]
        [FormerlySerializedAs("returnOnCompleted")]
        [SerializeField] protected bool _returnOnCompleted = true;

        [Tooltip("Time object takes to return to its starting position")]
        [FormerlySerializedAs("returnDuration")]
        [SerializeField] protected float _returnDuration = 1f;

        [Tooltip("Mouse texture to use when hovering mouse over object")]
        [FormerlySerializedAs("hoverCursor")]
        [SerializeField] protected Texture2D _hoverCursor;

        [Tooltip("Use the UI Event System to check for drag events. Clicks that hit " +
            "an overlapping UI object will be ignored. Camera must have a " +
            "PhysicsRaycaster component, or a Physics2DRaycaster for 2D colliders.")]
        [FormerlySerializedAs("useEventSystem")]
        [SerializeField] protected bool _useEventSystem;

        [FormerlySerializedAs("beingDragged")]
        [SerializeField] protected bool _beingDragged;

        public virtual bool BeingDragged
        {
            get { return _beingDragged; }
            set { _beingDragged = value; }
        }

        protected Vector3 startingPosition;
        protected bool updatePosition = false;
        protected Vector3 newPosition;
        protected Vector3 delta = Vector3.zero;

        // Amanita-owned runtime drag semantics: what targets this draggable is currently over.
        protected readonly HashSet<Collider2D> overlappingTargets = new HashSet<Collider2D>();

        protected virtual void LateUpdate()
        {
            // iTween will sometimes override the object position even if
            // it should only be affecting the scale, rotation, etc.
            // To make sure this doesn't happen, we force the position
            // change to happen in LateUpdate.
            if (updatePosition)
            {
                transform.position = newPosition;
                updatePosition = false;
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!_dragEnabled)
            {
                return;
            }

            if (_beingDragged && other != null)
            {
                overlappingTargets.Add(other);
            }

            var eventDispatcher = AmanitaManager.S.EventDispatcher;
            eventDispatcher.Raise(new DragEnteredEvent(this, other));
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!_dragEnabled)
            {
                return;
            }

            if (other != null)
            {
                overlappingTargets.Remove(other);
            }

            var eventDispatcher = AmanitaManager.S.EventDispatcher;
            eventDispatcher.Raise(new DragExitedEvent(this, other));
        }

        protected virtual void DoBeginDrag()
        {
            _beingDragged = true;
            overlappingTargets.Clear();

            // Offset the object so that the drag is anchored to the exact point where the user clicked it
#if ENABLE_INPUT_SYSTEM
            var mousePos = UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
#else
            var mousePos = Input.mousePosition;
#endif
            var screenPoint = new Vector3(mousePos.x, mousePos.y, 10f);
            delta = Camera.main.ScreenToWorldPoint(screenPoint) - transform.position;
            delta.z = 0f;

            startingPosition = transform.position;

            var eventDispatcher = AmanitaManager.S.EventDispatcher;
            eventDispatcher.Raise(new DragStartedEvent(this));
        }

        protected virtual void DoDrag()
        {
            if (!_dragEnabled)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            var mousePos = UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
#else
            var mousePos = Input.mousePosition;
#endif
            float x = mousePos.x;
            float y = mousePos.y;
            float z = transform.position.z;

            var screenPoint = new Vector3(x, y, 10f);
            newPosition = Camera.main.ScreenToWorldPoint(screenPoint) - delta;
            newPosition.z = z;
            updatePosition = true;
        }

        protected virtual void DoEndDrag()
        {
            if (!_dragEnabled)
            {
                return;
            }

            var eventDispatcher = AmanitaManager.S.EventDispatcher;

            // Ask listeners (bridge layer) whether this drag should count as completed.
            DragEndProbeEvent probeEvent = new DragEndProbeEvent(this, overlappingTargets);
            eventDispatcher.Raise(probeEvent);

            if (probeEvent.IsCompleted)
            {
                eventDispatcher.Raise(new DragCompletedEvent(this));

                if (_returnOnCompleted)
                {
                    Tweener.TweenPosition(gameObject.transform, gameObject.transform.position,
                        startingPosition, _returnDuration);
                }
            }
            else
            {
                eventDispatcher.Raise(new DragCancelledEvent(this));

                if (_returnOnCancelled)
                {
                    Tweener.TweenPosition(gameObject.transform, gameObject.transform.position,
                        startingPosition, _returnDuration);
                }
            }

            overlappingTargets.Clear();
            _beingDragged = false;
        }

        private DefaultTweenAdapter Tweener => TweenManager.S.DefaultAdapter;

        protected virtual void DoPointerEnter()
        {
            ChangeCursor(_hoverCursor);
        }

        protected virtual void DoPointerExit()
        {
            //SetMouseCursor.ResetMouseCursor();
        }

        protected virtual void ChangeCursor(Texture2D cursorTexture)
        {
            if (!_dragEnabled)
            {
                return;
            }

            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }

        #region Legacy OnMouseX methods

        protected virtual void OnMouseDown()
        {
            if (!_useEventSystem)
            {
                DoBeginDrag();
            }
        }

        protected virtual void OnMouseDrag()
        {
            if (!_useEventSystem)
            {
                DoDrag();
            }
        }

        protected virtual void OnMouseUp()
        {
            if (!_useEventSystem)
            {
                DoEndDrag();
            }
        }

        protected virtual void OnMouseEnter()
        {
            if (!_useEventSystem)
            {
                DoPointerEnter();
            }
        }

        protected virtual void OnMouseExit()
        {
            if (!_useEventSystem)
            {
                DoPointerExit();
            }
        }

        #endregion

        #region Public members

        /// <summary>
        /// Is object drag and drop enabled.
        /// </summary>
        /// <value><c>true</c> if drag enabled; otherwise, <c>false</c>.</value>
        public virtual bool DragEnabled { get { return _dragEnabled; } set { _dragEnabled = value; } }

        #endregion

        #region IBeginDragHandler implementation

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_useEventSystem)
            {
                DoBeginDrag();
            }
        }

        #endregion

        #region IDragHandler implementation

        public void OnDrag(PointerEventData eventData)
        {
            if (_useEventSystem)
            {
                DoDrag();
            }
        }

        #endregion

        #region IEndDragHandler implementation

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_useEventSystem)
            {
                DoEndDrag();
            }
        }

        #endregion

        #region IPointerEnterHandler implementation

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_useEventSystem)
            {
                DoPointerEnter();
            }
        }

        #endregion

        #region IPointerExitHandler implementation

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_useEventSystem)
            {
                DoPointerExit();
            }
        }

        #endregion
    }
}
