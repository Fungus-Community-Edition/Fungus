using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Calls a named method on a GameObject using the GameObject.SendMessage() system.
    /// This command is called "Call Method" because a) it's more descriptive than Send Message and we're already have
    /// a Send Message command for sending messages to trigger block execution.
    /// </summary>
    [CommandInfo("Scripting", 
                 "Call Method", 
                 "Calls a named method on a GameObject using the GameObject.SendMessage() system.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class CallMethod : Command
    {
        [Tooltip("Target monobehavior which contains the method we want to call")]
        [SerializeField] protected GameObject targetObject;

        [Tooltip("Name of the method to call")]
        [SerializeField] protected string methodName = "";

        [Tooltip("Delay (in seconds) before the method will be called")]
        [SerializeField] protected float delay;

        protected virtual void CallTheMethod()
        {
            targetObject.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }

        #region Public members

        public override void OnEnter()
        {
            if (targetObject == null ||
                methodName.Length == 0)
            {
                Continue();
                return;
            }

            if (Mathf.Approximately(delay, 0f))
            {
                CallTheMethod();
            }
            else
            {
                Invoke("CallTheMethod", delay);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetObject == null)
            {
                return "Error: No target GameObject specified";
            }

            if (methodName.Length == 0)
            {
                return "Error: No named method specified";
            }

            return targetObject.name + " : " + methodName;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        #endregion
    }
}
