using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Float variable type.
    /// </summary>
    [VariableInfo("Numeric", "Float", typeof(float))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class FloatVariable : VariableBase<float>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return true;
        }

        public override bool IsComparisonSupported()
        {
            return true;
        }

        public override void Apply(SetOperator setOperator, float value)
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
                Value *= value;
                break;
            case SetOperator.Divide:
                Value /= value;
                break;
            default:
                base.Apply(setOperator, value);
                break;
            }
        }

        public override bool Evaluate(CompareOperator compareOperator, float value)
        {
            float lhs = Value;
            float rhs = value;

            bool condition;

            switch (compareOperator)
            {
                case CompareOperator.LessThan:
                    condition = lhs < rhs;
                    break;
                case CompareOperator.GreaterThan:
                    condition = lhs > rhs;
                    break;
                case CompareOperator.LessThanOrEquals:
                    condition = lhs <= rhs;
                    break;
                case CompareOperator.GreaterThanOrEquals:
                    condition = lhs >= rhs;
                    break;
                default:
                    condition = base.Evaluate(compareOperator, value);
                    break;
            }

            return condition;
        }
    }

    /// <summary>
    /// Container for an float variable reference or constant value.
    /// </summary>
    [VariableData(typeof(float), typeof(FloatVariable))]
    [System.Serializable]
    public class FloatData : VariableData<float>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(FloatVariable))]
        public FloatVariable floatRef;
        public FloatData() : base(default) { }

        public FloatData(float startVal) : base(startVal)
        {
        }

        public static implicit operator float(FloatData floatData)
        {
            return floatData.Value;
        }

        public override IVariable VarRef
        {
            get { return floatRef; }
            set
            {
                if (value == null) { floatRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    floatRef = value as FloatVariable;
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