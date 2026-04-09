using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Command to execute and store the result of a Sqrt
    /// </summary>
    [CommandInfo("Math",
                 "Sqrt",
                 "Command to execute and store the result of a Sqrt")]
    [AddComponentMenu("")]
[MovedFrom("AtMycelia.Amanita.VScripting")]
    public class Sqrt : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Sqrt(inValue.Value);

            Continue();
        }
    }
}
