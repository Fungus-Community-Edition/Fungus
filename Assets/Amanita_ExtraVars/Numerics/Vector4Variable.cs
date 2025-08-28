




using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Vector4 variable type.
    /// </summary>
    [VariableInfo("Other", "Vector4")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Vector4Variable : VariableBase<UnityEngine.Vector4>
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
    public class Vector4Data : VariableData<Vector4, IVariable<Vector4>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Vector4Variable))]
        public Vector4Variable vector4Ref;

        public Vector4Data() : base(default) { }
        public Vector4Data(Vector4 startVal = default) : base(startVal) { }

        public override IVariable VarRef
        {
            get { return vector4Ref; }
            set
            {
                if (value == null) { vector4Ref = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    vector4Ref = value as Vector4Variable;
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }
    }
}