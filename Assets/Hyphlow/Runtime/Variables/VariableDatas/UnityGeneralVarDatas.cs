using UnityEngine;
using UnityObj = UnityEngine.Object;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [System.Serializable]
    [VariableData(typeof(Component), typeof(IVariable<Component>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ComponentData : VariableData<Component>
    {
        protected override Variable LegacyVarRef
        {
            get => null;
            set
            {

            }
        }
        protected override Component LegacyLiteralVal
        {
            get => null;
            set
            {

            }
        }
        public ComponentData() : base(default) { }
        public ComponentData(Component startVal = null) : base(startVal) { }
    }
    
    /// <summary>
    /// Container for a GameObject variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(GameObject), typeof(IVariable<GameObject>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class GameObjectData : VariableData<GameObject>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(GameObjectVariable))]
        public GameObjectVariable gameObjectRef;

        [SerializeField]
        public GameObject gameObjectVal;

        public GameObjectData() : base(default) { }
        public GameObjectData(GameObject startVal = null) : base(startVal) { }

        protected override Variable LegacyVarRef
        {
            get => gameObjectRef;
            set => gameObjectRef = value as GameObjectVariable;
        }

        protected override GameObject LegacyLiteralVal 
        {
            get => gameObjectVal;
            set => gameObjectVal = value;
        }

        public virtual T AddComponent<T>() where T : Component
        {
            if (this.Value == null)
            {
                Debug.LogError("Cannot add component to null GameObject reference.");
                return default;
            }
            GameObject go = this.Value;
            return go.AddComponent<T>();
        }

        public virtual string Name
        {
            get
            {
                GameObject go = this.Value;
                if (go != null)
                {
                    return go.name;
                }
                return null;
            }
            set
            {
                GameObject go = this.Value;
                if (go != null)
                {
                    go.name = value;
                }
            }
        }
        public virtual T GetComponent<T>()
        {
            GameObject go = this.Value;
            if (go != null)
            {
                return go.GetComponent<T>();
            }
            return default;
        }

        public virtual bool TryGetComponent<T>(out T result)
        {
            result = default;
            GameObject go = this.Value;
            if (go != null)
            {
                return go.TryGetComponent<T>(out result);
            }
            return false;
        }
    }

    /// <summary>
    /// Container for a Transform variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Transform), typeof(IVariable<Transform>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class TransformData : VariableData<Transform>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TransformVariable))]
        public TransformVariable transformRef;

        [SerializeField]
        public Transform transformVal;

        protected override Variable LegacyVarRef
        {
            get => transformRef;
            set => transformRef = value as TransformVariable;
        }

        protected override Transform LegacyLiteralVal
        {
            get => transformVal;
            set => transformVal = value;
        }

        public TransformData() : base(default) { }
        public TransformData(Transform startVal = null) : base(startVal) { }

        public override IVariable VarRef
        {
            get
            {
                return transformRef != null ? 
                    transformRef : 
                    base.VarRef;
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
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ObjectData : VariableData<UnityObj>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(ObjectVariable))]
        public ObjectVariable objectRef;

        [SerializeField]
        public Object objectVal;

        protected override Variable LegacyVarRef
        {
            get => objectRef;
            set => objectRef = value as ObjectVariable;
        }

        protected override UnityObj LegacyLiteralVal
        {
            get => objectVal;
            set => objectVal = value;
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