using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Multiplicative Inverse of a float (1/f)
    /// </summary>
    [CommandInfo("Math",
                 "Inverse",
                 "Multiplicative Inverse of a float (1/f)")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Inv : BaseUnaryMathCommand
    {
        public override void OnEnter()
        {
            var v = inValue.Value;

            outValue.Value = v != 0 ? (1.0f / inValue.Value) : 0.0f;

            Continue();
        }
    }
}
