using UnityEngine;
using System;
using UnityEngine.Events;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Supported types of method invocation.
    /// </summary>
    public enum InvokeType
    {
        /// <summary> Call a method with an optional constant value parameter. </summary>
        Static,         // 
        /// <summary> Call a method with an optional boolean constant / variable parameter. </summary>
        DynamicBoolean,
        /// <summary> Call a method with an optional integer constant / variable parameter. </summary>
        DynamicInteger,
        /// <summary> Call a method with an optional float constant / variable parameter. </summary>
        DynamicFloat,
        /// <summary> Call a method with an optional string constant / variable parameter. </summary>
        DynamicString
    }

    /// <summary>
    /// Calls a list of component methods via the Unity Event System (as used in the Unity UI)
    /// This command is more efficient than the Invoke Method command but can only pass a single parameter and doesn't support return values.
    /// This command uses the UnityEvent system to call methods in script. http://docs.unity3d.com/Manual/UnityEvents.html
    /// </summary>
    [CommandInfo("Scripting", 
                 "Invoke Event", 
                 "Calls a list of component methods via the Unity Event System (as used in the Unity UI). " + 
                 "This command is more efficient than the Invoke Method command but can only pass a single parameter and doesn't support return values.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class InvokeEvent : Command
    {
        [Tooltip("A description of what this command does. Appears in the command summary.")]
        [SerializeField] protected string description = "";

        [Tooltip("Delay (in seconds) before the methods will be called")]
        [SerializeField] protected float delay;

        [Tooltip("Selects type of method parameter to pass")]
        [SerializeField] protected InvokeType invokeType;

        [Tooltip("List of methods to call. Supports methods with no parameters or exactly one string, int, float or object parameter.")]
        [SerializeField] protected UnityEvent staticEvent = new UnityEvent();

        [Tooltip("Boolean parameter to pass to the invoked methods.")]
        [SerializeField] protected BooleanData booleanParameter;

        [Tooltip("List of methods to call. Supports methods with one boolean parameter.")]
        [SerializeField] protected BooleanEvent booleanEvent = new BooleanEvent();

        [Tooltip("Integer parameter to pass to the invoked methods.")]
        [SerializeField] protected IntegerData integerParameter;
        
        [Tooltip("List of methods to call. Supports methods with one integer parameter.")]
        [SerializeField] protected IntegerEvent integerEvent = new IntegerEvent();

        [Tooltip("Float parameter to pass to the invoked methods.")]
        [SerializeField] protected FloatData floatParameter;
        
        [Tooltip("List of methods to call. Supports methods with one float parameter.")]
        [SerializeField] protected FloatEvent floatEvent = new FloatEvent();

        [Tooltip("String parameter to pass to the invoked methods.")]
        [SerializeField] protected StringDataMulti stringParameter;

        [Tooltip("List of methods to call. Supports methods with one string parameter.")]
        [SerializeField] protected StringEvent stringEvent = new StringEvent();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(booleanParameter);
            _variableDataCache.Add(integerParameter);
            _variableDataCache.Add(floatParameter);
            _variableDataCache.Add(stringParameter);
        }

        protected virtual void DoInvoke()
        {
            switch (invokeType)
            {
                default:
                case InvokeType.Static:
                    staticEvent.Invoke();
                    break;
                case InvokeType.DynamicBoolean:
                    booleanEvent.Invoke(booleanParameter.Value);
                    break;
                case InvokeType.DynamicInteger:
                    integerEvent.Invoke(integerParameter.Value);
                    break;
                case InvokeType.DynamicFloat:
                    floatEvent.Invoke(floatParameter.Value);
                    break;
                case InvokeType.DynamicString:
                    stringEvent.Invoke(stringParameter.Value);
                    break;
            }
        }

        #region Public members

        [Serializable] public class BooleanEvent : UnityEvent<bool> {}
        [Serializable] public class IntegerEvent : UnityEvent<int> {}
        [Serializable] public class FloatEvent : UnityEvent<float> {}
        [Serializable] public class StringEvent : UnityEvent<string> {}

        public override void OnEnter()
        {
            if (Mathf.Approximately(delay, 0f))
            {
                DoInvoke();
            }
            else
            {
                Invoke("DoInvoke", delay);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }

            string summary = invokeType.ToString() + " ";

            switch (invokeType)
            {
            default:
            case InvokeType.Static:
                summary += staticEvent.GetPersistentEventCount();
                break;
            case InvokeType.DynamicBoolean:
                summary += booleanEvent.GetPersistentEventCount();
                break;
            case InvokeType.DynamicInteger:
                summary += integerEvent.GetPersistentEventCount();
                break;
            case InvokeType.DynamicFloat:
                summary += floatEvent.GetPersistentEventCount();
                break;
            case InvokeType.DynamicString:
                summary += stringEvent.GetPersistentEventCount();
                break;
            }

            return summary + " methods";
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(booleanParameter.VarRef, variable) || 
                ReferenceEquals(integerParameter.VarRef, variable) ||
                ReferenceEquals(floatParameter.VarRef, variable) || 
                ReferenceEquals(stringParameter.VarRef, variable) ||
                base.HasReference(variable);
        }

        #endregion
    }
}
