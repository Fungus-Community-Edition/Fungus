using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Negate a float
    /// </summary>
    [CommandInfo("Math",
                 "Negate",
                 "Negate a float")]
    [AddComponentMenu("")]
    public class Neg : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            outValue.Value = -(inValue.Value);

            Continue();
        }
    }
}
