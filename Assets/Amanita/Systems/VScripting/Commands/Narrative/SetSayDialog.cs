using UnityEngine;
using Amanita.DialogueSys;

namespace Amanita.VScripting.Commands.Legacy
{
    /// <summary>
    /// Sets a custom say dialog to use when displaying story text.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Set Say Dialog", 
                 "Sets a custom say dialog to use when displaying story text")]
    [AddComponentMenu("")]
    public class SetSayDialog : Command 
    {
        [Tooltip("The Say Dialog to use for displaying Say story text")]
        [SerializeField] protected SayDialog sayDialog;

        #region Public members

        public override void OnEnter()
        {
            if (sayDialog != null)
            {
                SayDialog.ActiveSayDialog = sayDialog;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (sayDialog == null)
            {
                return "Error: No say dialog selected";
            }

            return sayDialog.name;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Narrative;
        }

        #endregion
    }
}