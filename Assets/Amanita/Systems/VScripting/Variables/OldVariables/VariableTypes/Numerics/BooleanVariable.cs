using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Boolean variable type.
    /// </summary>
    [VariableInfo("Numeric", "Boolean", typeof(bool))]
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
    [VariableData(typeof(bool), typeof(BooleanVariable))]
    public class BooleanData : VariableData<bool, IVariable<bool>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(BooleanVariable))]
        public BooleanVariable booleanRef;

        [SerializeField]
        public bool booleanVal;

        public BooleanData() : base(default) { }
        public BooleanData(bool startVal = default) : base(startVal) { }

        public static implicit operator bool(BooleanData booleanData)
        {
            return booleanData.Value;
        }

        public override IVariable VarRef
        {
            get { return booleanRef; }
            set
            {
                if (value == null) { booleanRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    booleanRef = value as BooleanVariable;
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