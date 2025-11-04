using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Vector2 variable type.
    /// </summary>
    [VariableInfo("Numeric", "Vector2", typeof(Vector2), false)]
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
    }

}
