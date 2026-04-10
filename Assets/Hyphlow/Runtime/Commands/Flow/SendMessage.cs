using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Supported target types for messages.
    /// </summary>
    public enum MessageTarget
    {
        /// <summary>
        /// Send message to the Flowchart containing the SendMessage command.
        /// </summary>
        SameFlowchart,
        /// <summary>
        /// Broadcast message to all Flowcharts.
        /// </summary>
        AllFlowcharts
    }

    /// <summary>
    /// Sends a message to either the owner Flowchart or all Flowcharts in the scene. Blocks can listen for this message using a Message Received event handler.
    /// </summary>
    [CommandInfo("Flow", 
                 "Send Message", 
                 "Sends a message to either the owner Flowchart or all Flowcharts in the scene." +
        "Blocks can listen for this message using a Message Received event handler.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SendMessage : Command
    {
        [Tooltip("Target flowchart(s) to send the message to")]
        [SerializeField] protected MessageTarget messageTarget;

        [Tooltip("Name of the message to send")]
        [SerializeField] protected StringData _message = new StringData("");

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_message);
        }

        #region Public members

        public override void OnEnter()
        {
            if (_message.Value.Length == 0)
            {
                Continue();
                return;
            }

            MessageReceived[] receivers = null;
            if (messageTarget == MessageTarget.SameFlowchart)
            {
                receivers = GetComponents<MessageReceived>();
            }
            else
            {
            #if UNITY_6000
                receivers = GameObject.FindObjectsByType<MessageReceived>(FindObjectsSortMode.None);
            #else
                receivers = GameObject.FindObjectsOfType<MessageReceived>();
            #endif
            }

            if (receivers != null)
            {
                for (int i = 0; i < receivers.Length; i++)
                {
                    var receiver = receivers[i];
                    receiver.OnSendFungusMessage(_message.Value);
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (_message.Value.Length == 0)
            {
                return "Error: No message specified";
            }
            
            return _message.Value;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_message.VarRef, variable) || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("message")] public string messageOLD = "";

        protected override void OnEnable()
        {
            base.OnEnable();
            if (messageOLD != "")
            {
                _message.Value = messageOLD;
                messageOLD = "";
            }
        }

        #endregion
    }
}