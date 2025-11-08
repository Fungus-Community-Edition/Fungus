using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Rigidbody variable type.
    /// </summary>
    [VariableInfo("Physics/ThreeD", "Rigidbody", typeof(Rigidbody))]
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
        public RigidbodyData() : base(default) { }

        public RigidbodyData(Rigidbody startVal) : base(startVal)
        {
        }
    }
}