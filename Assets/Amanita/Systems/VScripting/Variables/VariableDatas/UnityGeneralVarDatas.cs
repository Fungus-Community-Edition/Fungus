using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    /// <summary>
    /// Container for a GameObject variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(GameObject), typeof(IVariable<GameObject>))]
    public class GameObjectData : VariableData<GameObject>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(GameObjectVariable))]
        public GameObjectVariable gameObjectRef;

        public GameObjectData() : base(default) { }
        public GameObjectData(GameObject startVal = null) : base(startVal) { }

        public override void Refresh()
        {
            varRef ??= gameObjectRef;
        }
    }

    /// <summary>
    /// Container for a Transform variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Transform), typeof(IVariable<Transform>))]
    public class TransformData : VariableData<Transform>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(TransformVariable))]
        public TransformVariable transformRef;

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

    /// <summary>
    /// Container for an Object variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(UnityObj), typeof(IVariable<UnityObj>))]
    public class ObjectData : VariableData<UnityObj>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(ObjectVariable))]
        public ObjectVariable objectRef;

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
                return varRef ?? objectRef;
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