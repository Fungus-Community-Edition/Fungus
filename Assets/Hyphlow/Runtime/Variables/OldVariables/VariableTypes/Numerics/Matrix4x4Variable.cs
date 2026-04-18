using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Matrix4x4 variable type.
    /// </summary>
    [AddComponentMenu("")]
    [System.Serializable]
    public class Matrix4x4Variable : VariableBase<Matrix4x4>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return setOperator == SetOperator.Multiply || base.IsArithmeticSupported(setOperator);
        }

        public override void Apply(SetOperator setOperator, Matrix4x4 value)
        {
            Matrix4x4 local = Value;

            switch (setOperator)
            {
                case SetOperator.Add:
                    Debug.LogWarning("Matrix4x4 Add not supported");
                    break;

                case SetOperator.Subtract:
                    Debug.LogWarning("Matrix4x4 Subtract not supported");
                    break;

                case SetOperator.Multiply:
                    Value = local * value;
                    break;

                case SetOperator.Divide:
                    Debug.LogWarning("Matrix4x4 Divide not supported");
                    break;

                default:
                base.Apply(setOperator, value);
                break;
            }
        }
    }

    /// <summary>
    /// Container for a Matrix4x4 variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Matrix4x4), typeof(Matrix4x4Variable))]
    public class Matrix4x4Data : VariableData<Matrix4x4>
    {
        public Matrix4x4Data() : base(default) { }

        public Matrix4x4Data(Matrix4x4 startVal) : base(startVal)
        {
        }

    }
}