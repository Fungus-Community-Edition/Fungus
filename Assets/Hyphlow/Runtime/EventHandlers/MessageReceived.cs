using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// The block will execute when the specified message is received from a Send Message command.
    /// </summary>
    [EventHandlerInfo("Scene",
                      "Message Received",
                      "The block will execute when the specified message is received from a Send Message command.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class MessageReceived : EventHandler 
    {
        [Tooltip("Fungus message to listen for")]
        [SerializeField]
        protected StringData _message = new StringData(string.Empty);


        [FormerlySerializedAs("message")]
        [HideInInspector]
        [SerializeField] protected string _oldMessage = "";

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (!string.IsNullOrEmpty(_oldMessage))
            {
                _message.Value = _oldMessage;
                _oldMessage = "";
            }
        }
        
        #region Public members

        /// <summary>
        /// Called from Flowchart when a message is sent.
        /// </summary>
        /// <param name="message">Message.</param>
        public void OnSendFungusMessage(string message)
        {
            if (this._message.Value == message)
            {
                ExecuteBlock();
            }
        }

        public override string GetSummary()
        {
            return _message.Value;
        }

        #endregion
    }
}