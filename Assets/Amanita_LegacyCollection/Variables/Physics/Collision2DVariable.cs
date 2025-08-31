using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Collision2D variable type.
    /// </summary>
    [VariableInfo("Physics", "Collision2D", typeof(Collision2D), IsPreviewedOnly = true)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class Collision2DVariable : VariableBase<UnityEngine.Collision2D>
    { }

    [System.Serializable]
    [VariableData(typeof(Collision2D), typeof(Collision2DVariable))]
    public class Collision2DData : VariableData<Collision2D, IVariable<Collision2D>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(Collision2DVariable))]
        public Collision2DVariable collision2dRef;

        public static implicit operator Collision2D(Collision2DData Collision2DData)
        {
            return Collision2DData.Value;
        }

        public Collision2DData() : base(default) { }

        public Collision2DData(Collision2D startVal) : base(startVal) { }

        public override IVariable VarRef
        {
            get { return collision2dRef; }
            set
            {
                if (value == null) { collision2dRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    collision2dRef = value as Collision2DVariable;
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