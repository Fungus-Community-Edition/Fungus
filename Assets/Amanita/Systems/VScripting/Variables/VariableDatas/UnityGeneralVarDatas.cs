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
        [SerializeField]
        [VariableProperty("<Value>", typeof(GameObjectVariable))]
        public GameObjectVariable gameObjectRef;

        public GameObjectData() : base(default) { }
        public GameObjectData(GameObject startVal = null) : base(startVal) { }

        protected override Variable LegacyVarRef
        {
            get => gameObjectRef;
            set => gameObjectRef = value as GameObjectVariable;
        }
    }

    /// <summary>
    /// Container for a Transform variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Transform), typeof(IVariable<Transform>))]
    public class TransformData : VariableData<Transform>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TransformVariable))]
        public TransformVariable transformRef;

        protected override Variable LegacyVarRef
        {
            get => transformRef;
            set => transformRef = value as TransformVariable;
        }

        public TransformData() : base(default) { }
        public TransformData(Transform startVal = null) : base(startVal) { }

        public override IVariable VarRef
        {
            get
            {
                // Prefer legacy field for compatibility
                return transformRef != null ? transformRef : base.VarRef;
            }
            set
            {
                if (value == null) { transformRef = null; base.VarRef = null; return; }

                if (this.ContentType.IsAssignableFrom(value.ContentType))
                {
                    if (value is UnityObj)
                    {
                        // Unity Object must be kept in legacy object field
                        transformRef = value as TransformVariable;
                        base.VarRef = null;
                    }
                    else
                    {
                        // Pure managed IVariable<T>
                        base.VarRef = value;
                        transformRef = null;
                    }
                }
                else
                {
                    throw new System.InvalidCastException($"This can only accept a variable type that holds content of type {ContentType.Name}.");
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
        [SerializeField]
        [VariableProperty("<Value>", typeof(ObjectVariable))]
        public ObjectVariable objectRef;

        protected override Variable LegacyVarRef
        {
            get => objectRef;
            set => objectRef = value as ObjectVariable;
        }

        public ObjectData() : base(default) { }
        public ObjectData(UnityObj startVal = null) : base(startVal) { }

        public override IVariable VarRef
        {
            get
            {
                return objectRef != null ? objectRef : base.VarRef;
            }
            set
            {
                if (value == null) { objectRef = null; base.VarRef = null; return; }

                if (this.ContentType.IsAssignableFrom(value.ContentType))
                {
                    if (value is UnityObj)
                    {
                        objectRef = value as ObjectVariable;
                        base.VarRef = null;
                    }
                    else
                    {
                        base.VarRef = value;
                        objectRef = null;
                    }
                }
                else
                {
                    throw new System.InvalidCastException($"This can only accept a variable type that holds content of type {ContentType.Name}.");
                }
            }
        }
    }
}