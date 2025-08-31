using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Vector3 variable type.
    /// </summary>
    [VariableInfo("Numeric", "Vector3", typeof(Vector3))]
    [AddComponentMenu("")]
    [System.Serializable]
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

    /// <summary>
    /// Container for a Vector3 variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Vector3), typeof(Vector3Variable))]
    public class Vector3Data : VariableData<Vector3, IVariable<Vector3>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Vector3Variable))]
        public Vector3Variable vector3Ref;
        
        public static implicit operator Vector3(Vector3Data vector3Data)
        {
            return vector3Data.Value;
        }

        public Vector3Data() : base(default) { }
        public Vector3Data(Vector3 startVal = default) : base(startVal) { }

        public override IVariable VarRef
        {
            get { return vector3Ref; }
            set
            {
                if (value == null) { vector3Ref = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    vector3Ref = value as Vector3Variable;
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