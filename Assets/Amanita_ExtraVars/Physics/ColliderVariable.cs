




using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Collider variable type.
    /// </summary>
    [VariableInfo("Other", "Collider")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ColliderVariable : VariableBase<UnityEngine.Collider>
    { }

    /// <summary>
    /// Container for a Collider variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Collider), typeof(ColliderVariable))]
    public class ColliderData : VariableData<Collider, IVariable<Collider>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(ColliderVariable))]
        public ColliderVariable colliderRef;

        [SerializeField]
        public UnityEngine.Collider colliderVal;

        public ColliderData() : base(default) { }

        public ColliderData(Collider startVal) : base(startVal)
        {
        }

        public override IVariable VarRef
        {
            get { return colliderRef; }
            set
            {
                if (value == null) { colliderRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    colliderRef = value as ColliderVariable;
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