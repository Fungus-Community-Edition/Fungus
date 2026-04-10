using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Gets the state of a toggle UI object and stores it in a boolean variable.
    /// </summary>
    [CommandInfo("UI",
                 "Get Toggle State",
                 "Gets the state of a toggle UI object and stores it in a boolean variable.")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class GetToggleState : Command 
    {
        [Tooltip("Target toggle object to get the value from")]
        [SerializeField] protected Toggle toggle;

        [Tooltip("Boolean variable to store the state of the toggle value in.")]
        [VariableProperty(typeof(BooleanVariable))]
        [SerializeField] protected BooleanVariable toggleState;

        #region Public members

        public override void OnEnter() 
        {
            if (toggle != null &&
                toggleState != null)
            {
                toggleState.Value = toggle.isOn;
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

            if (toggleState == null)
            {
                return "Error: Toggle state variable not selected";
            }

            return toggle.name;
        }

        public override bool HasReference(Variable variable)
        {
            return toggleState == variable || 
                base.HasReference(variable);
        }

        #endregion
    }
}
