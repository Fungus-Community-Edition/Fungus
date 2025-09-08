using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Vector2 variable type.
    /// </summary>
    [VariableInfo("Numeric", "Vector2", typeof(Vector2))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Vector2Variable : VariableBase<Vector2>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return true;
        }

        public override void Apply(SetOperator setOperator, Vector2 value)
        {
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
#if UNITY_2019_2_OR_NEWER
                Value *= value;
#else
                var tmpM = Value;
                tmpM.Scale(value);
                Value = tmpM;
#endif
                break;
            case SetOperator.Divide:
#if UNITY_2019_2_OR_NEWER
                Value /= value;
#else
                var tmpD = Value;
                tmpD.Scale(new Vector2(1.0f / value.x, 1.0f / value.y));
                Value = tmpD;
#endif
                break;
            default:
                base.Apply(setOperator, value);
                break;
            }
        }
    }

    /// <summary>
    /// Container for a Vector2 variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Vector2), typeof(Vector2Variable))]
    public class Vector2Data : VariableData<Vector2>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Vector2Variable))]
        public Vector2Variable vector2Ref;

        public Vector2Data() : base(default) { }
        public Vector2Data(Vector2 startVal = default) : base(startVal) { }

        public static implicit operator Vector2(Vector2Data vector2Data)
        {
            return vector2Data.Value;
        }

        public override IVariable VarRef
        {
            get { return vector2Ref; }
            set
            {
                if (value == null) { vector2Ref = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    vector2Ref = value as Vector2Variable;
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