using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Marks the start of a command block to be executed when the preceding If statement is False and the test expression is true.
    /// </summary>
    [CommandInfo("Flow", 
                 "Lua Else If", 
                 "Marks the start of a command block to be executed when the preceding If statement is False and the test expression is true.")]
    [AddComponentMenu("")]
    public class LuaElseIf : LuaCondition
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