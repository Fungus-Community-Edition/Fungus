using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Vector4 variable type.
    /// </summary>
    [System.Serializable]
    public class Vector4Variable : VariableBase<Vector4>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return true;
        }

        public override void Apply(SetOperator setOperator, Vector4 value)
        {
            Vector4 local = Value;

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
                    local.Scale(new Vector4(1.0f / value.x, 1.0f / value.y, 1.0f / value.z, 1.0f / value.w));
                    Value = local;
                    break;

                default:
                base.Apply(setOperator, value);
                break;
            }
        }
    }

    /// <summary>
    /// Container for a Vector4 variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Vector4), typeof(Vector4Variable))]
    public class Vector4Data : VariableData<Vector4>
    {
        public Vector4Data() : base(default) { }
        public Vector4Data(Vector4 startVal = default) : base(startVal) { }

    }
}