using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Collision variable type.
    /// </summary>
    [VariableInfo("Physics", "Collision", typeof(Collision), IsPreviewedOnly = true)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class CollisionVariable : VariableBase<UnityEngine.Collision>
    { }

    [System.Serializable]
    [VariableData(typeof(Collision), typeof(CollisionVariable))]
    public class CollisionData : VariableData<Collision, IVariable<Collision>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(CollisionVariable))]
        public CollisionVariable collisionRef;

        public CollisionData() : base(default) { }

        public CollisionData(Collision startVal) : base(startVal)
        {
        }

        public override IVariable VarRef
        {
            get { return collisionRef; }
            set
            {
                if (value == null) { collisionRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    collisionRef = value as CollisionVariable;
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