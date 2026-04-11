using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Continuously loop through a block of commands while the condition is true. Use the Break command to force the loop to terminate immediately.
    /// </summary>
    [CommandInfo("Flow", 
                 "While", 
                 "Continuously loop through a block of commands while the condition is true. Use the Break command to force the loop to terminate immediately.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class While : If
    {
        #region Public members

        public override bool IsLooping { get { return true; } }

        #endregion
    }    
}