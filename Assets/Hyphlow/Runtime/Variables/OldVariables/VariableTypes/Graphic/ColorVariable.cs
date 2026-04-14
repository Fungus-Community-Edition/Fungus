using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Color variable type.
    /// </summary>
    [VariableInfo("Graphic", "Color", typeof(Color), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ColorVariable : VariableBase<Color>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return setOperator != SetOperator.Negate;
        }

        public override void Apply(SetOperator setOperator, Color value)
        {
            switch (setOperator)
            {
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
                Value *= new Color(1.0f/value.r, 1.0f / value.g, 1.0f / value.b, 1.0f / value.a);
                break;
            default:
                base.Apply(setOperator, value);
                break;
            }
        }
    }

    
}
