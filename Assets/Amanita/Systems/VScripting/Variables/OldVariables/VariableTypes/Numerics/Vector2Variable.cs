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
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(Vector2Variable))]
        public IVariable<Vector2> vector2Ref;

        public Vector2Data() : base(default) { }
        public Vector2Data(Vector2 startVal = default) : base(startVal) { }

        public override void Refresh()
        {
            varRef ??= vector2Ref;
        }
    }
}