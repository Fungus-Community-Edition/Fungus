


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Boolean variable type.
    /// </summary>
    [VariableInfo("", "Boolean")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class BooleanVariable : VariableBase<bool>
    {
        public override bool IsArithmeticSupported(SetOperator setOperator)
        {
            return setOperator == SetOperator.Negate || base.IsArithmeticSupported(setOperator);
        }

        public override void Apply(SetOperator op, bool value)
        {
            switch (op)
            {
            case SetOperator.Negate:
                Value = !value;
                break;
            default:
                base.Apply(op, value);
                break;
            }
        }
    }

    /// <summary>
    /// Container for a Boolean variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public class BooleanData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(BooleanVariable))]
        public BooleanVariable booleanRef;

        [SerializeField]
        public bool booleanVal;

        public BooleanData(bool startVal = false)
        {
            booleanVal = startVal;
            booleanRef = null;
        }
        
        public static implicit operator bool(BooleanData booleanData)
        {
            return booleanData.Value;
        }

        public bool Value
        {
            get { return (booleanRef == null) ? booleanVal : booleanRef.Value; }
            set { if (booleanRef == null) { booleanVal = value; } else { booleanRef.Value = value; } }
        }

        public string GetDescription()
        {
            if (booleanRef == null)
            {
                return booleanVal.ToString();
            }
            else
            {
                return booleanRef.Key;
            }
        }
    }
}