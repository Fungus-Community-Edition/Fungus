using UnityEngine;
using Amanita.VScripting;

namespace Amanita.DialogueSys.Commands
{
    /// <summary>
    /// Clears the options from a menu dialogue.
    /// </summary>
    [CommandInfo("Narrative",
                 "Clear Menu",
                 "Clears the options from a menu dialogue")]
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
