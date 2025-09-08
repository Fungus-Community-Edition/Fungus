using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Collider2D variable type.
    /// </summary>
    [VariableInfo("Physics", "Collider2D", typeof(Collider2D))]
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
        [SerializeField]
        [VariableProperty("<Value>", typeof(Collider2DVariable))]
        public Collider2DVariable collider2DRef;

        public Collider2DData() : base(default) { }

        public Collider2DData(Collider2D startVal) : base(startVal)
        {
        }

        public override IVariable VarRef
        {
            get { return collider2DRef; }
            set
            {
                if (value == null) { collider2DRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
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