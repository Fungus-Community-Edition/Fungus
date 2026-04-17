using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Rigidbody variable type.
    /// </summary>
    [VariableInfo("Physics/ThreeD", "Rigidbody", typeof(Rigidbody))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class RigidbodyVariable : VariableBase<UnityEngine.Rigidbody>
    { }

}