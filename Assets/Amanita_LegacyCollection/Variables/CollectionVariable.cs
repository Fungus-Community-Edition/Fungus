using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Collection variable type.
    /// </summary>
    [VariableInfo("Other", "Collection", typeof(Collection))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class CollectionVariable : VariableBase<Collection>
    { }

    /// <summary>
    /// Container for a Collection variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Collection), typeof(CollectionVariable))]
    public class CollectionData : VariableData<Collection>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(CollectionVariable))]
        public IVariable<Collection> collectionRef;

        [SerializeField]
        public Collection collectionVal;

        public CollectionData() : base(default) { }
        public CollectionData(Collection startVal) : base(startVal) { }

        public override void Refresh()
        {
            backingVarRef.Variable ??= collectionRef;
        }

        public override IVariable VarRef
        {
            get
            {
                // Prefer the protected serialized varRef (it may be a VariablePointer<T>), but fall back to the old derived objectRef.
                return backingVarRef.Variable ?? collectionRef;
            }
            set
            {
                if (value == null) { backingVarRef.Variable = null; collectionRef = null; return; }

                // Accept any variable whose ContentType is assignable to UnityObj (polymorphism allowed).
                if (this.ContentType.IsAssignableFrom(value.ContentType))
                {
                    // Keep the protected varRef consistent with whatever is passed in (covers VariablePointer<T> cases).
                    backingVarRef.Variable = value;

                    collectionRef = value as CollectionVariable;
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