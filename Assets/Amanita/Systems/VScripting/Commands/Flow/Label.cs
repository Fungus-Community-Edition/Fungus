using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Marks a position in the command list for execution to jump to.
    /// </summary>
    [CommandInfo("Flow", 
                 "Label", 
                 "Marks a position in the command list for execution to jump to.")]
    [AddComponentMenu("")]
    public class Label : Command
    {
        [Tooltip("Display name for the label")]
        [SerializeField] protected string key = "";

        #region Public members

        /// <summary>
        /// Display name for the label
        /// </summary>
        public virtual string Key { get { return key; } }

        public override void OnEnter()
        {
            Continue();
        }

        public override string GetSummary()
        {
            return key;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Label;
        }

        #endregion
    }
}
