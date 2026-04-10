using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Command to execute and store the result of a Exp
    /// </summary>
    [CommandInfo("Math",
                 "Exp",
                 "Command to execute and store the result of a Exp")]
    [AddComponentMenu("")]
[MovedFrom("AtMycelia.Hyphlow")]
    public class Exp : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Exp(inValue.Value);

            Continue();
        }
    }
}
