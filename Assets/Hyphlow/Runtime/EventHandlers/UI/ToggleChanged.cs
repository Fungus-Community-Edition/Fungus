using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// The block will execute when the user toggles on the target UI toggle object.
    /// </summary>
    [EventHandlerInfo("UI",
                      "Toggle Changed",
                      "The block will execute when the state of the target UI " +
                      "toggle object changes. The state of the toggle is stored in the " +
                      "Toggle State boolean variable.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class ToggleChanged : EventHandler
    {   
        [Tooltip("The block will execute when the state of the target UI toggle object changes.")]
        [SerializeField] protected Toggle targetToggle;

        [Tooltip("The new state of the UI toggle object is stored in this boolean variable.")]
        [VariableProperty(typeof(BooleanVariable))]
        [SerializeField] protected BooleanVariable toggleState;

        #region Public members

        public virtual void Start()
        {
            if (targetToggle != null)
            {
                targetToggle.onValueChanged.AddListener(OnToggleChanged);
            }
        }
        
        protected virtual void OnToggleChanged(bool state)
        {
            if (toggleState != null)
            {
                toggleState.Value = state;
            }

            ExecuteBlock();
        }

        public override string GetSummary()
        {
            if (targetToggle != null)
            {
                return targetToggle.name;
            }

            return "Error: No targetToggle set.";
        }

        #endregion
    }
}
