using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Writes a log message to the debug console.
    /// </summary>
    [CommandInfo("Scripting",
                 "Debug Break",
                 "Calls Debug.Break if enabled. Also useful for putting a visual studio breakbpoint within.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class DebugBreak : Command
    {
        [SerializeField] new protected BooleanData enabled = new BooleanData(true);

        public override void OnEnter()
        {
            if (enabled.Value)
                Debug.Break();

            Continue();
        }

        public override string GetSummary()
        {
            return enabled.Value ? "enabled" : "disabled";
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(variable, enabled.VarRef);
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }
    }
}
