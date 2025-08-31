using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Command to execute and store the result of a Exp
    /// </summary>
    [CommandInfo("Math",
                 "Exp",
                 "Command to execute and store the result of a Exp")]
    [AddComponentMenu("")]
    public class Exp : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = Mathf.Exp(inValue.Value);

            Continue();
        }
    }
}
