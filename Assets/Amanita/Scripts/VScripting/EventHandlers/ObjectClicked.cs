using UnityEngine;
using System.Collections;
using AtMycelia.Hyphlow;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// The block will execute when the user clicks or taps on the clickable object.
    /// </summary>
    [EventHandlerInfo("Sprite",
                      "Object Clicked",
                      "The block will execute when the user clicks or taps on the clickable object.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class ObjectClicked : EventHandler
    {   
        public class ObjectClickedEvent
        {
            public Clickable2D ClickableObject;
            public ObjectClickedEvent(Clickable2D clickableObject)
            {
                ClickableObject = clickableObject;
            }
        }

        [Tooltip("Object that the user can click or tap on")]
        [SerializeField] protected Clickable2D clickableObject;

        [Tooltip("Wait for a number of frames before executing the block.")]
        [SerializeField] protected int waitFrames = 1;

        protected EventDispatcher eventDispatcher;

        protected override void OnEnable()
        {
            eventDispatcher = FindFirstObjectByType<EventDispatcher>();
            base.OnEnable();
        }

        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);
            if (on)
            {
                eventDispatcher.AddListener<ObjectClickedEvent>(OnObjectClickedEvent);
            }
            else
            {
                eventDispatcher.RemoveListener<ObjectClickedEvent>(OnObjectClickedEvent);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            eventDispatcher = null;
        }

        void OnObjectClickedEvent(ObjectClickedEvent evt)
        {
            OnObjectClicked(evt.ClickableObject);
        }

        /// <summary>
        /// Called by the Clickable2D object when it is clicked.
        /// </summary>
        public virtual void OnObjectClicked(Clickable2D clickableObject)
        {
            if (clickableObject == this.clickableObject)
            {
                StartCoroutine(DoExecuteBlock(waitFrames));
            }
        }

        /// <summary>
        /// Executing a block on the same frame that the object is clicked can cause
        /// input problems (e.g. auto completing Say Dialog text). A single frame delay 
        /// fixes the problem.
        /// </summary>
        protected virtual IEnumerator DoExecuteBlock(int numFrames)
        {
            if (numFrames == 0)
            {
                ExecuteBlock();
                yield break;
            }

            int count = Mathf.Max(waitFrames, 1);
            while (count > 0)
            {
                count--;
                yield return new WaitForEndOfFrame();
            }

            ExecuteBlock();
        }

        

        public override string GetSummary()
        {
            if (clickableObject != null)
            {
                return clickableObject.name;
            }

            return "Error: no clickableObject set.";
        }

    }
}
