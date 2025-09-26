using UnityEngine;

namespace Amanita.VScripting.Commands
{
    /// <summary>
    /// Continuously loop through a block of commands while the condition is true. Use the Break command to force the loop to terminate immediately.
    /// </summary>
    [CommandInfo("Flow", 
                 "While", 
                 "Continuously loop through a block of commands while the condition is true. Use the Break command to force the loop to terminate immediately.")]
    [AddComponentMenu("")]
    public class While : If
    {
        #region Public members

        public override bool IsLooping { get { return true; } }

        #endregion
    }    
}