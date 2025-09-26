using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Transform variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "Transform", typeof(Transform))]
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
    public class TransformData : VariableData<Transform>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(TransformVariable))]
        public IVariable<Transform> transformRef;

        public TransformData() : base(default) { }
        public TransformData(Transform startVal = null) : base(startVal) { }

        public override void Refresh()
        {
            varRef ??= transformRef;
        }

        public override IVariable VarRef
        {
            get
            {
                // Prefer the protected serialized varRef (it may be a VariablePointer<T>), but fall back to the old derived objectRef.
                return varRef ?? transformRef;
            }
            set
            {
                if (value == null) { varRef = null; transformRef = null; return; }

                // Accept any variable whose ContentType is assignable to UnityObj (polymorphism allowed).
                if (this.ContentType.IsAssignableFrom(value.ContentType))
                {
                    // Keep the protected varRef consistent with whatever is passed in (covers VariablePointer<T> cases).
                    varRef = value;

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