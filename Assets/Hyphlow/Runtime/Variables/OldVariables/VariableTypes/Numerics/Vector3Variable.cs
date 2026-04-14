using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Vector3 variable type.
    /// </summary>
    [VariableInfo("Numeric", "Vector3", typeof(Vector3), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Vector3Variable : VariableBase<Vector3>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return true;
        }

        public override void Apply(SetOperator setOperator, Vector3 value)
        {
            Vector3 local = Value;

            switch (setOperator)
            {
            case SetOperator.Negate:
                Value = Value * -1;
                break;
            case SetOperator.Add:
                Value += value;
                break;
            case SetOperator.Subtract:
                Value -= value;
                break;
            case SetOperator.Multiply:
                local.Scale(value);
                Value = local;
                break;
            case SetOperator.Divide:
                local.Scale(new Vector3(1.0f / value.x, 1.0f / value.y, 1.0f / value.z));
                Value = local;
                break;
            default:
                base.Apply(setOperator, value);
                break;
            }
        }
    }

}
