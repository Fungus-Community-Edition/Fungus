


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Transform variable type.
    /// </summary>
    [VariableInfo("Other", "Transform")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class TransformVariable : VariableBase<Transform>
    {
    }

    /// <summary>
    /// Container for a Transform variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Transform), typeof(TransformVariable))]
    public class TransformData : VariableData<Transform, IVariable<Transform>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TransformVariable))]
        public TransformVariable transformRef;

        public static implicit operator Transform(TransformData vector3Data)
        {
            return vector3Data.Value;
        }

        public TransformData() : base(default) { }
        public TransformData(Transform startVal = null) : base(startVal) { }

        public override IVariable VarRef
        {
            get { return transformRef; }
            set
            {
                if (value == null) { transformRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    transformRef = value as TransformVariable;
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