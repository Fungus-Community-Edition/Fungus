using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Marks the start of a command block to be executed when the preceding If statement is False and the test expression is true.
    /// </summary>
    [CommandInfo("Flow", 
                 "Else If", 
                 "Marks the start of a command block to be executed when the preceding If statement is False and the test expression is true.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class ElseIf : VariableCondition
    {
        protected override bool IsElseIf { get { return true; } }

        #region Public members

        public override bool CloseBlock()
        {
            return true;
        }

        #endregion
    }
}