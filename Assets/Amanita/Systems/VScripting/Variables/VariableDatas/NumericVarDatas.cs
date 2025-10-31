using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Container for an integer variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(int), typeof(IVariable<int>))]
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

    /// <summary>
    /// Container for an float variable reference or constant value.
    /// </summary>
    [VariableData(typeof(float), typeof(IVariable<float>))]
    [System.Serializable]
    public class FloatData : VariableData<float>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(FloatVariable))]
        public FloatVariable floatRef;
        public FloatData() : base(default) { }

        public FloatData(float startVal) : base(startVal)
        {
        }

        public override void Refresh()
        {
            varRef ??= floatRef;
        }

    }

    /// <summary>
    /// Container for a Boolean variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(bool), typeof(IVariable<bool>))]
    public class BooleanData : VariableData<bool>
    {
        [SerializeField, SerializeReference]
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

        public override void Refresh()
        {
            varRef ??= booleanRef;
        }
    }
}