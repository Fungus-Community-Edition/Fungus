using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Float variable type.
    /// </summary>
    [VariableInfo("Numeric", "Float", typeof(float), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
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

        protected override object FilteredForValueSet(object valueToConvert)
        {
            float result = (float)Convert.ToSingle(valueToConvert);
            return result;
        }
    }

}
