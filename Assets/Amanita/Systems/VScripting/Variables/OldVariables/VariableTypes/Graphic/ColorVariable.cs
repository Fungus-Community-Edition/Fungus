using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Color variable type.
    /// </summary>
    [VariableInfo("Graphic", "Color", typeof(Color))]
    [AddComponentMenu("")]
    [System.Serializable]
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

    /// <summary>
    /// Container for a Color variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Color), typeof(ColorVariable))]
    public class ColorData : VariableData<Color>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(ColorVariable))]
        public IVariable<Color> colorRef;

        public ColorData() : base(default) { }
        public ColorData(Color startVal = default) : base(startVal) { }

        public static implicit operator Color(ColorData colorData)
        {
            return colorData.Value;
        }

        public override void Refresh()
        {
            varRef ??= colorRef;
        }

    }
}