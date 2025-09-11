using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Integer variable type.
    /// </summary>
    [VariableInfo("Numeric", "Integer", typeof(int))]
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
    public class IntegerData : VariableData<int>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(IntegerVariable))]
        public IntegerVariable integerRef;

        public IntegerData() : base(default) { }

        public IntegerData(int startVal) : base(startVal)
        {
        }

        public override void Refresh()
        {
            varRef ??= integerRef;
        }

    }
}