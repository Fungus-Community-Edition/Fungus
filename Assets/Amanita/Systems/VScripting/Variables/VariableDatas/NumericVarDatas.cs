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
        [SerializeField]
        [VariableProperty("<Value>", typeof(IntegerVariable))]
        public IntegerVariable integerRef;
        protected override Variable LegacyVarRef
        {
            get => integerRef;
            set
            {
                integerRef = value as IntegerVariable;
                base.LegacyVarRef = value;
            }
        }

        public IntegerData() : base(default) { }

        public IntegerData(int startVal) : base(startVal)
        {
        }
    }

    /// <summary>
    /// Container for an float variable reference or constant value.
    /// </summary>
    [VariableData(typeof(float), typeof(IVariable<float>))]
    [System.Serializable]
    public class FloatData : VariableData<float>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(FloatVariable))]
        public FloatVariable floatRef;
        protected override Variable LegacyVarRef
        {
            get => floatRef;
            set
            {
                floatRef = value as FloatVariable;
                base.LegacyVarRef = value;
            }
        }
        public FloatData() : base(default) { }

        public FloatData(float startVal) : base(startVal)
        {
        }
    }

    /// <summary>
    /// Container for a Boolean variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(bool), typeof(IVariable<bool>))]
    public class BooleanData : VariableData<bool>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(BooleanVariable))]
        public BooleanVariable booleanRef;

        protected override Variable LegacyVarRef
        {
            get => booleanRef;
            set => booleanRef = value as BooleanVariable;
        }

        [SerializeField]
        public bool booleanVal;

        public BooleanData() : base(default) { }
        public BooleanData(bool startVal = default) : base(startVal) { }

        public static implicit operator bool(BooleanData booleanData)
        {
            return booleanData.Value;
        }
    }

    /// <summary>
    /// Container for a Vector2 variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Vector2), typeof(IVariable<Vector2>))]
    public class Vector2Data : VariableData<Vector2>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Vector2Variable))]
        public Vector2Variable vector2Ref;

        public Vector2Data() : base(default) { }
        public Vector2Data(Vector2 startVal = default) : base(startVal) { }

        protected override Variable LegacyVarRef
        {
            get => vector2Ref;
            set => vector2Ref = value as Vector2Variable;
        }
    }

    /// <summary>
    /// Container for a Vector3 variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Vector3), typeof(IVariable<Vector3>))]
    public class Vector3Data : VariableData<Vector3>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Vector3Variable))]
        public Vector3Variable vector3Ref;

        public Vector3Data() : base(default) { }
        public Vector3Data(Vector3 startVal = default) : base(startVal) { }

        protected override Variable LegacyVarRef
        {
            get => vector3Ref;
            set => vector3Ref = value as Vector3Variable;
        }
    }
}