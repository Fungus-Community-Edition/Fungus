using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    /// <summary>
    /// Object variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "UnityObject", typeof(UnityObj))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ObjectVariable : VariableBase<UnityObj>
    {
    }

    /// <summary>
    /// Container for an Object variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(UnityObj), typeof(ObjectVariable))]
    public class ObjectData : VariableData<UnityObj>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(ObjectVariable))]
        public IVariable<UnityObj> objectRef;

        public ObjectData() : base(default) { }
        public ObjectData(UnityObj startVal = null) : base(startVal) { }
        
        // Ensure runtime/backing-field synchronization when serialized fields are manipulated
        public override void Refresh()
        {
            // Try to populate the derived field from the serialized protected varRef if possible.
            // Note: varRef may be a VariablePointer<T> (for legacy vars of more specific Unity types).
            // We won't force a cast from VariablePointer<T> to IVariable<UnityObj> here — keep objectRef only
            // when it's truly an ObjectVariable. Always ensure the base protected varRef is populated if objectRef exists.
            objectRef ??= varRef as ObjectVariable;
            varRef ??= objectRef;
        }

        public override IVariable VarRef
        {
            get
            {
                // Prefer the protected serialized varRef (it may be a VariablePointer<T>), but fall back to the old derived objectRef.
                return varRef ?? (IVariable)objectRef;
            }
            set
            {
                if (value == null) { varRef = null; objectRef = null; return; }

                // Accept any variable whose ContentType is assignable to UnityObj (polymorphism allowed).
                if (this.ContentType.IsAssignableFrom(value.ContentType))
                {
                    // Keep the protected varRef consistent with whatever is passed in (covers VariablePointer<T> cases).
                    varRef = value;
                    // If it's directly an ObjectVariable, populate objectRef for older code paths that use it.
                    objectRef = value as ObjectVariable;
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