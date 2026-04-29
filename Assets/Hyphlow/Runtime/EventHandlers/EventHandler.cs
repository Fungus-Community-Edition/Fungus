using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
namespace AtMycelia.Hyphlow
{
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
    public class EventHandler : MonoBehaviour, ISerializationCallbackReceiver, IBackwardsCompatibilityApplier
    {
        [HideInInspector]
        [FormerlySerializedAs("parentSequence")]
        [FormerlySerializedAs("parentBlock")]
        [SerializeField] protected Block _parentBlock;

        [Tooltip("If true, the flowchart window will not auto select the Block when the Event " +
            "Handler fires. Affects Editor only.")]
        [FormerlySerializedAs("suppressBlockAutoSelect")]
        [SerializeField] protected bool _suppressBlockAutoSelect = false;

        protected virtual void Awake()
        {
            _fChart = GetComponent<Flowchart>();
        }

        #region Public members

        /// <summary>
        /// The parent Block which owns this Event Handler.
        /// </summary>
        public virtual Block ParentBlock
        {
            get => _parentBlock;
            set
            {
                _parentBlock = value;
                _fChart = null;
                if (_parentBlock != null)
                {
                    _fChart = _parentBlock.GetFlowchart();
                }
            }
        }

        protected Flowchart _fChart;
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
            if (_fChart == null || !this.gameObject.activeInHierarchy || !_fChart.isActiveAndEnabled)
            {
                return false;
            }

            if (_suppressBlockAutoSelect)
            {
                ParentBlock.SuppressNextAutoSelection = true;
            }

            return _fChart.ExecuteBlock(ParentBlock);
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
            if (ToggleSubsOnlyInRuntime && Application.IsPlaying(this))
            {
                ToggleSubs(false);
            }
            else if (!ToggleSubsOnlyInRuntime)
            {
                ToggleSubs(false);
            }
        }

        protected virtual void OnValidate()
        {
            if (!this.IsInTheScene)
            {
                return;
            }
            // Seems that when this is set to execute in edit mode, OnValidate can be called
            // before Awake does. Thus, we need to ensure fChart is assigned.
            if (_fChart == null)
            {
                _fChart = GetComponent<Flowchart>();
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
                HyphlowManager manager = HyphlowManager.S;
                if (manager == null)
                {
                    return null;
                }
                return manager.EventDispatcher;
            }
        }

        public virtual void ApplyBackwardsCompatibility()
        {

        }
    }
}
