using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Command to execute and store the result of a Abs
    /// </summary>
    [CommandInfo("Math",
                 "Abs",
                 "Command to execute and store the result of a Abs")]
    [AddComponentMenu("")]
[MovedFrom("AtMycelia.Hyphlow")]
    public class Abs : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Abs(inValue.Value);

            Continue();
        }
    }
}
