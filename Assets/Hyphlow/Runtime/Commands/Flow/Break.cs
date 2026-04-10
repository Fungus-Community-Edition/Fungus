using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Force a loop to terminate immediately.
    /// </summary>
    [CommandInfo("Flow",
                 "Break",
                 "Force a loop to terminate immediately.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Break : Command
    {
        #region Public members

        //located the containing loop and tell it to end
        public override void OnEnter()
        {
            Condition loopingCond = null;
            // Find index of previous looping command
            for (int i = CommandIndex - 1; i >= 0; --i)
            {
                Condition cond = ParentBlock.CommandList[i] as Condition;
                if (cond != null && cond.IsLooping)
                {
                    loopingCond = cond;
                    break;
                }
            }

            if (loopingCond == null)
            {
                // No enclosing loop command found, just continue
                Debug.LogError("Break called but found no enclosing looping construct." + GetLocationIdentifier());
                Continue();
            }
            else
            {
                loopingCond.MoveToEnd();
            }
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        #endregion
    }    
}
