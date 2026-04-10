using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets the state of a toggle UI object.
    /// </summary>
    [CommandInfo("UI",
                 "Set Toggle State",
                 "Sets the state of a toggle UI object")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetToggleState : Command 
    {
        [Tooltip("Target toggle object to set the state on")]
        [SerializeField] protected Toggle toggle;

        [Tooltip("Boolean value to set the toggle state to.")]
        [SerializeField] protected BooleanData value;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(value);
        }

        #region Public members

        public override void OnEnter() 
        {
            if (toggle != null)
            {
                toggle.isOn = value.Value;
            }

            Continue();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override string GetSummary()
        {
            if (toggle == null)
            {
                return "Error: Toggle object not selected";
            }

            return toggle.name + " = " + value.GetDescription();
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(value.VarRef, variable) || base.HasReference(variable);
        }

        #endregion
    }
}
