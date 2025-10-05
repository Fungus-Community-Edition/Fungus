using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Stop executing the Block that contains this command.
    /// </summary>
    [CommandInfo("Flow", 
                 "Stop", 
                 "Stop executing the Block that contains this command.")]
    [AddComponentMenu("")]
    public class Stop : Command
    {
        #region Public members

        public override void OnEnter()
        {
            StopParentBlock();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        #endregion
    }
}
