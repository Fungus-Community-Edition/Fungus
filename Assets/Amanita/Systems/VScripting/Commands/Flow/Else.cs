using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Marks the start of a command block to be executed when the preceding If statement is False.
    /// </summary>
    [CommandInfo("Flow", 
                 "Else", 
                 "Marks the start of a command block to be executed when the preceding If statement is False.")]
    [AddComponentMenu("")]
    public class Else : Command
    {
        #region Public members

        public override void OnEnter()
        {
            // Find the next End command at the same indent level as this Else command
            var matchingEnd = Condition.FindMatchingEndCommand(this);
            if (matchingEnd != null)
            {
                // Execute command immediately after the EndIf command
                Continue(matchingEnd.CommandIndex + 1);
            }
            else
            {
                // No End command found
                StopParentBlock();
            }
        }

        public override bool OpenBlock()
        {
            return true;
        }

        public override bool CloseBlock()
        {
            return true;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.ConditionalLogic;
        }

        #endregion
    }
}
