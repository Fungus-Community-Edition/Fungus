using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.VScripting.EventHandlers
{
    /// <summary>
    /// The block will execute when the player starts dragging an object.
    /// </summary>
    [EventHandlerInfo("Sprite",
                      "Drag Started",
                      "The block will execute when the player starts dragging an object.")]
    [AddComponentMenu("")]
    public class DragStarted : EventHandler, ISerializationCallbackReceiver
    {
        public class DragStartedEvent
        {
            public Draggable2D DraggableObject;

            public DragStartedEvent(Draggable2D draggableObject)
            {
                DraggableObject = draggableObject;
            }
        }

        [VariableProperty(typeof(GameObjectVariable))]
        [SerializeField] protected GameObjectVariable draggableRef;

        [SerializeField] protected List<Draggable2D> draggableObjects;

        [HideInInspector]
        [SerializeField] protected Draggable2D draggableObject;

        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);

            if (on)
            {
                EventDispatcher.AddListener<DragStartedEvent>(OnDragStartedEvent);
            }
            else
            {
                EventDispatcher.RemoveListener<DragStartedEvent>(OnDragStartedEvent);
            }
        }

        private void OnDragStartedEvent(DragStartedEvent evt)
        {
            OnDragStarted(evt.DraggableObject);
        }

        #region Compatibility

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            //add any dragableobject already present to list for backwards compatability
            if (draggableObject != null)
            {
                if (!draggableObjects.Contains(draggableObject))
                {
                    draggableObjects.Add(draggableObject);
                }
                draggableObject = null;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        #endregion Compatibility

        #region Public members

        /// <summary>
        /// Called by the Draggable2D object when the drag starts.
        /// </summary>
        public virtual void OnDragStarted(Draggable2D draggableObject)
        {
            if (draggableObjects.Contains(draggableObject))
            {
                if (draggableRef != null)
                {
                    draggableRef.Value = draggableObject.gameObject;
                }
                ExecuteBlock();
            }
        }

        public override string GetSummary()
        {
            if(draggableObjects.Count(x=> x != null) == 0)
            {
                return "Error: no draggable objects assigned.";
            }

            string summary = "Draggable: ";
            if (this.draggableObjects != null && this.draggableObjects.Count != 0)
            {
                for (int i = 0; i < this.draggableObjects.Count; i++)
                {
                    if (draggableObjects[i] != null)
                    {
                        summary += draggableObjects[i].name + ",";
                    }
                }
            }

            return summary;
        }

        #endregion Public members
    }
}