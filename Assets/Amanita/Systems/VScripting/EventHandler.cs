using System;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita.VScripting.EventHandlers
{
    /// <summary>
    /// Attribute class for Fungus event handlers.
    /// </summary>
    public class EventHandlerInfoAttribute : Attribute
    {
        public EventHandlerInfoAttribute(string category, string eventHandlerName, string helpText)
        {
            this.Category = category;
            this.EventHandlerName = eventHandlerName;
            this.HelpText = helpText;
        }
        
        public string Category { get; set; }
        public string EventHandlerName { get; set; }
        public string HelpText { get; set; }
    }

    /// <summary>
    /// A Block may have an associated Event Handler which starts executing commands when
    /// a specific event occurs. 
    /// To create a custom Event Handler, simply subclass EventHandler and call the ExecuteBlock() method
    /// when the event occurs. 
    /// Add an EventHandlerInfo attibute and your new EventHandler class will automatically appear in the
    /// 'Execute On Event' dropdown menu when a block is selected.
    /// </summary>
    [RequireComponent(typeof(Block))]
    [RequireComponent(typeof(Flowchart))]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class EventHandler : MonoBehaviour, ISerializationCallbackReceiver
    {   
        [HideInInspector]
        [FormerlySerializedAs("parentSequence")]
        [SerializeField] protected Block parentBlock;

        [Tooltip("If true, the flowchart window will not auto select the Block when the Event Handler fires. Affects Editor only.")]
        [SerializeField] protected bool suppressBlockAutoSelect = false;

        protected virtual void Awake()
        {
            fChart = GetComponent<Flowchart>();
        }

        #region Public members
        
        /// <summary>
        /// The parent Block which owns this Event Handler.
        /// </summary>
        public virtual Block ParentBlock
        {
            get => parentBlock;
            set
            {
                parentBlock = value;
                fChart = null;
                if (parentBlock != null)
                {
                    fChart = parentBlock.GetFlowchart();
                }
            }
        }

        protected Flowchart fChart;
        /// <summary>
        /// The Event Handler should call this method in response to the relevant event occurring.
        /// </summary>
        public virtual bool ExecuteBlock()
        {
            if (ParentBlock == null)
            {
                return false;
            }

            if (ParentBlock._EventHandler != this)
            {
                return false;
            }

            //if somehow the flowchart is invalid or has been disabled we don't want to continue
            if (fChart == null || !fChart.isActiveAndEnabled)
            {
                return false;
            }

            if (suppressBlockAutoSelect)
            {
                ParentBlock.SuppressNextAutoSelection = true;
            }

            return fChart.ExecuteBlock(ParentBlock);
        }

        /// <summary>
        /// Returns custom summary text for the event handler.
        /// </summary>
        public virtual string GetSummary()
        {
            return "";
        }

        #endregion

        protected virtual void OnEnable()
        {
            if (this == null || !this.IsInTheScene)
            {
                return;
            }

            if (ToggleSubsOnlyInRuntime && Application.IsPlaying(this))
            {
                ToggleSubs(true);
            }
            else if (!ToggleSubsOnlyInRuntime)
            {
                ToggleSubs(true);
            }
        }

        // We want subclasses to have control of when they sub. Some would prefer to only
        // sub in runtime, so...
        protected virtual bool ToggleSubsOnlyInRuntime => true;

        /// <summary>
        /// Enable or disable any subscriptions to events.
        /// </summary>
        protected virtual void ToggleSubs(bool on)
        {

        }

        private bool IsInTheScene => gameObject.scene.IsValid() && !string.IsNullOrEmpty(gameObject.scene.name);

#if UNITY_EDITOR
        public virtual string DisplayNameAboveBlock
        {
            get
            {
                var eventHandlerInfo = GetType().GetCustomAttribute<EventHandlerInfoAttribute>();
                if (eventHandlerInfo != null)
                {
                    return eventHandlerInfo.EventHandlerName;
                }
                return GetType().Name;
            }
        }
#endif

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

        protected virtual void OnValidate()
        {
            if (!this.IsInTheScene)
            {
                return;
            }
            // Seems that when this is set to execute in edit mode, OnValidate can be called
            // before Awake does. Thus, we need to ensure fChart is assigned.
            if (fChart == null)
            {
                fChart = GetComponent<Flowchart>();
            }
        }

        public virtual void OnBeforeSerialize()
        {
            
        }

        public virtual void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null)
                {
                    return;
                }
                OnAfterDeserializeBackwardsCompat();
            };
#endif
        }

        protected virtual void OnAfterDeserializeBackwardsCompat()
        {

        }
        protected virtual EventDispatcher EventDispatcher
        {
            get
            {
                if (AmanitaManager.S != null)
                {
                    return AmanitaManager.S.EventDispatcher;
                }
                return null;
            }
        }
        
    }
}
