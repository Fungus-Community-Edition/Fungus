using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Increases the FungusPriority count, causing the related FungusPrioritySignals to fire.
    /// Intended to be used to notify external systems that fungus is doing something important and they should perhaps pause.
    /// </summary>
    [CommandInfo("PrioritySignals",
                 "Priority Up",
                 "Increases the FungusPriority count, causing the related FungusPrioritySignals to fire. " +
                "Intended to be used to notify external systems that fungus is doing something important and they should perhaps pause.")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class FungusPriorityIncrease : Command
    {
        public override void OnEnter()
        {
            FungusPrioritySignals.DoIncreasePriorityDepth();

            Continue();
        }
    }
}
