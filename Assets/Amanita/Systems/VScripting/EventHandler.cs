using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace Amanita.VScripting.EventHandlers
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
    public class EventHandler : MonoBehaviour
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
            if(fChart == null || !fChart.isActiveAndEnabled)
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
            if (ToggleSubsOnlyInRuntime && Application.IsPlaying(this))
            {
                ToggleSubs(true);
            }
            else if (!ToggleSubsOnlyInRuntime)
            {
                ToggleSubs(true);
            }

            if (RehydrateVarInputs)
            {
                DoRehydrationProcess();
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

        protected virtual bool RehydrateVarInputs => false;

        protected virtual void DoRehydrationProcess()
        {
            CacheVarFieldsFor(GetType());
            // ^Need to do this in both OnEnable and OnValidate, because doing it only in the latter
            // gets us a null ref error when Unity starts up the project
            bool weAreInTheEditor = !Application.isPlaying;
            if (weAreInTheEditor)
            {
                RehydrateVariables();
                return;
            }

            // In runtime, we only need each EventHandler to rehydrate once. Letting them do so
            // more than once can waste valuable clock cycles, what with how we're using reflection.
            bool shouldRehydrateDuringRuntime = !didRuntimeRehydration && Application.IsPlaying(this);
            if (shouldRehydrateDuringRuntime)
            {
                RehydrateVariables();
                didRuntimeRehydration = true;
            }
        }

        private static readonly Dictionary<Type, FieldInfo[]> typeFieldCache = new();

        protected static void CacheVarFieldsFor(Type ourType)
        {
            bool alreadyCachedForIt = typeFieldCache.ContainsKey(ourType);
            if (alreadyCachedForIt)
            {
                return;
            }

            var cacheForOurType = ourType.GetFields(fieldSearchFlags)
                    .Where(MightNeedRehydration)
                    .ToArray();
            typeFieldCache[ourType] = cacheForOurType;
        }

        protected static readonly BindingFlags fieldSearchFlags = BindingFlags.Instance |
            BindingFlags.NonPublic | BindingFlags.Public;

        protected static bool MightNeedRehydration(FieldInfo fieldInfo)
        {
            bool implementsIVariable = iVariableType.IsAssignableFrom(fieldInfo.FieldType);
            bool result = HasTheRightAttributes(fieldInfo) && implementsIVariable;
                
            return result;
        }

        protected static Type iVariableType = typeof(IVariable);

        private static bool HasTheRightAttributes(FieldInfo field)
        {
            return field.GetCustomAttribute<VariablePropertyAttribute>() != null &&
                field.GetCustomAttribute<SerializeReference>() != null;
        }

        protected void RehydrateVariables()
        {
            bool ourTypeIsCachedFor = typeFieldCache.TryGetValue(GetType(), out var varFieldCache);
            if (!ourTypeIsCachedFor)
            {
                Debug.LogError($"Tried to rehydrate variables in {GetType().Name} before caching its variable fields.");
                return;
            }

            foreach (var fieldEl in varFieldCache)
            {
                RehydrateField(fieldEl, this, fChart);
            }
        }

        protected static void RehydrateField(FieldInfo field, object target, Flowchart fChart)
        {
            IVariable varToCheck = (IVariable)field.GetValue(target);
            // ^We already filtered the var fields based on implementing IVariable, 
            // and thus this cast should always succeed.
            if (varToCheck == null)
            {
                // This happens when the var is a legacy one that got semi-nulled, which would be why the
                // hard cast above still works.
                return;
            }
            bool alreadyHydrated = varToCheck.Owner != null;
            if (alreadyHydrated)
            {
                return;
            }

            if (fChart == null)
            {
                Debug.LogError($"Cannot rehydrate variable {field.Name} because Flowchart is null.");
                return;
            }
            var correct = fChart.GetVariableById(varToCheck.ItemId);
            if (correct == null)
            {
                Debug.LogError($"Variable {field.Name} in (Flowchart {fChart.name}) not found.");
                return;
            }
            field.SetValue(target, correct);
        }

        protected bool didRuntimeRehydration = false;

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

        protected virtual void OnValidate()
        {
            // Seems that when this is set to execute in edit mode, OnValidate can be called
            // before Awake does. Thus, we need to ensure fChart is assigned.
            if (fChart == null)
            {
                fChart = GetComponent<Flowchart>();
            }
            if (RehydrateVarInputs)
            {
                DoRehydrationProcess();
            }
        }

        protected virtual EventDispatcher EventDispatcher => AmanitaManager.S.EventDispatcher;
        
    }
}
