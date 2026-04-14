using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Integer variable type.
    /// </summary>
    [VariableInfo("Numeric", "Integer", typeof(int), false)]
    [AddComponentMenu("")]
    [Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
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

        protected override object FilteredForValueSet(object valueToConvert)
        {
            int result = (int)Convert.ToSingle(valueToConvert);
            return result;
        }
    }

}
