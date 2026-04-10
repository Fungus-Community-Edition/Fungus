using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Command to execute and store the result of a Sign
    /// </summary>
    [CommandInfo("Math",
                 "Sign",
                 "Command to execute and store the result of a Sign")]
    [AddComponentMenu("")]
[MovedFrom("AtMycelia.Hyphlow")]
    public class Sign : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Sign(inValue.Value);

            Continue();
        }
    }
}
