using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Boolean variable type.
    /// </summary>
    [VariableInfo("Numeric", "Boolean", typeof(bool), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class BooleanVariable : VariableBase<bool>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return setOperator == SetOperator.Negate || base.IsArithmeticSupported(setOperator);
        }

        public override void Apply(SetOperator op, bool value)
        {
            switch (op)
            {
            case SetOperator.Negate:
                Value = !value;
                break;
            default:
                base.Apply(op, value);
                break;
            }
        }
    }

}
