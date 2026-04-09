using UnityEngine;
using AtMycelia.Hyphlow;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.DialogueSys.VScripting
{
    /// <summary>
    /// Clears the options from a menu dialogue.
    /// </summary>
    [CommandInfo("Narrative",
                 "Clear Menu",
                 "Clears the options from a menu dialogue")]
    [MovedFrom("AtMycelia.Amanita.DialogueSys.VScripting.Commands")]
    public class ClearMenu : Command 
    {
        [Tooltip("Menu Dialog to clear the options on")]
        [SerializeField] protected MenuDialog menuDialog;

        #region Public members

        public override void OnEnter()
        {
            menuDialog.Clear();

            Continue();
        }

        public override string GetSummary()
        {
            if (menuDialog == null)
            {
                return "Error: No menu dialog object selected";
            }
            
            return menuDialog.name;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Narrative;
        }

        #endregion
    }
}
