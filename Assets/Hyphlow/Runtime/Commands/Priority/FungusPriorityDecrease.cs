using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Decrease the FungusPriority count, causing the related FungusPrioritySignals to fire.
    /// Intended to be used to notify external systems that fungus is doing something important and they should perhaps resume.
    /// </summary>
    [CommandInfo("PrioritySignals",
                 "Priority Down",
                 "Decrease the FungusPriority count, causing the related FungusPrioritySignals to fire. " +
                "Intended to be used to notify external systems that fungus is doing something important and they should perhaps resume.")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class FungusPriorityDecrease : Command
    {
        public override void OnEnter()
        {
            FungusPrioritySignals.DoDecreasePriorityDepth();

            Continue();
        }
    }
}
