using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Resets the FungusPriority count to zero. Useful if you are among logic that is hard to have matching increase and decreases.
    /// </summary>
    [CommandInfo("PrioritySignals",
                 "Priority Reset",
                 "Resets the FungusPriority count to zero. Useful if you are among logic that is hard to have matching increase and decreases.")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class FungusPriorityReset : Command
    {
        public override void OnEnter()
        {
            FungusPrioritySignals.DoResetPriority();

            Continue();
        }
    }
}
