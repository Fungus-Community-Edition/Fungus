using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Marks a position in the command list for execution to jump to.
    /// </summary>
    [CommandInfo("Flow", 
                 "Label", 
                 "Marks a position in the command list for execution to jump to.")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Label : Command
    {
        [Tooltip("Display name for the label")]
        [SerializeField] protected StringData _key = new StringData("");

        #region Public members

        /// <summary>
        /// Display name for the label
        /// </summary>
        public virtual string Key { get { return _key; } }

        public override void OnEnter()
        {
            Continue();
        }

        public override string GetSummary()
        {
            string result = _key.Value;
            if (_key.RepresentingVar)
            {
                result = $"{_key.VarRef.Key} ({_key.Value})";
            }
            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Label;
        }

        #endregion

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (!string.IsNullOrEmpty(key))
            {
                _key.Value = key;
                key = "";
            }
        }

        [HideInInspector]
        [SerializeField] protected string key = "";
    }
}
