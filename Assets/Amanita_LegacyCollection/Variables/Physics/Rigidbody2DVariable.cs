


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Rigidbody2D variable type.
    /// </summary>
    [VariableInfo("Physics", "Rigidbody2D", typeof(Rigidbody2D))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Rigidbody2DVariable : VariableBase<Rigidbody2D>
    {
    }

    /// <summary>
    /// Container for a Rigidbody2D variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Rigidbody2D), typeof(Rigidbody2DVariable))]
    public class Rigidbody2DData : VariableData<Rigidbody2D, IVariable<Rigidbody2D>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Rigidbody2DVariable))]
        public Rigidbody2DVariable rigidbody2DRef;

        public Rigidbody2DData() : base(default) { }

        public Rigidbody2DData(Rigidbody2D startVal) : base(startVal)
        {
        }

        public override IVariable VarRef
        {
            get { return rigidbody2DRef; }
            set
            {
                if (value == null) { rigidbody2DRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    rigidbody2DRef = value as Rigidbody2DVariable;
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