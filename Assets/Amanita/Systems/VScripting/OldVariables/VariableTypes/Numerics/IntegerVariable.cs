


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Integer variable type.
    /// </summary>
    [VariableInfo("", "Integer", "Integer")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class IntegerVariable : VariableBase<int>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return true;
        }

        public override bool IsComparisonSupported()
        {
            return true;
        }

        public override void Apply(SetOperator setOperator, int value)
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

        public override bool Evaluate(CompareOperator compareOperator, int value)
        {
            int lhs = Value;
            int rhs = value;

            bool condition = false;

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
    /// Container for an integer variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(int), typeof(IntegerVariable))]
    public class IntegerData : VariableData<int, IVariable<int>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(IntegerVariable))]
        public IntegerVariable integerRef;

        public IntegerData() : base(default) { }

        public IntegerData(int startVal) : base(startVal)
        {
        }

        public static implicit operator int(IntegerData integerData)
        {
            return integerData.Value;
        }

        public override IVariable VarRef
        {
            get { return integerRef; }
            set
            {
                if (value == null || integerRef == null)
                {
                    integerRef = null; return;
                }

                if (VarRef.ContentType.Equals(value.ContentType))
                {
                    integerRef = value as IntegerVariable;
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