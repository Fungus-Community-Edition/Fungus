using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Collider2D variable type.
    /// </summary>
    [VariableInfo("Physics/TwoD", "Collider2D", typeof(Collider2D))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Collider2DVariable : VariableBase<UnityEngine.Collider2D>
    { }

    /// <summary>
    /// Container for a Collider2D variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Collider2D), typeof(Collider2DVariable))]
    public class Collider2DData : VariableData<Collider2D>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(Collider2DVariable))]
        public IVariable<Collider2D> collider2DRef;

        public Collider2DData() : base(default) { }

        public Collider2DData(Collider2D startVal) : base(startVal)
        {
        }

        public override void Refresh()
        {
            backingVarRef.Variable ??= collider2DRef;
        }

        public override IVariable VarRef
        {
            get
            {
                // Prefer the protected serialized backingVarRef.Variable (it may be a VariablePointer<T>), but fall back to the old derived objectRef.
                return backingVarRef.Variable ?? collider2DRef;
            }
            set
            {
                if (value == null) { backingVarRef.Variable = null; collider2DRef = null; return; }

                // Accept any variable whose ContentType is assignable to UnityObj (polymorphism allowed).
                if (this.ContentType.IsAssignableFrom(value.ContentType))
                {
                    // Keep the protected backingVarRef.Variable consistent with whatever is passed in (covers VariablePointer<T> cases).
                    backingVarRef.Variable = value;

                    collider2DRef = value as Collider2DVariable;
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