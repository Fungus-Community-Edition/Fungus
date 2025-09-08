using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Rigidbody variable type.
    /// </summary>
    [VariableInfo("Physics", "Rigidbody", typeof(Rigidbody))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class RigidbodyVariable : VariableBase<UnityEngine.Rigidbody>
    { }

    /// <summary>
    /// Container for a Rigidbody variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Rigidbody), typeof(RigidbodyVariable))]
    public class RigidbodyData : VariableData<Rigidbody>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(RigidbodyVariable))]
        public RigidbodyVariable rigidbodyRef;

        public RigidbodyData() : base(default) { }

        public RigidbodyData(Rigidbody startVal) : base(startVal)
        {
        }

        public override IVariable VarRef
        {
            get { return rigidbodyRef; }
            set
            {
                if (value == null) { rigidbodyRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    rigidbodyRef = value as RigidbodyVariable;
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