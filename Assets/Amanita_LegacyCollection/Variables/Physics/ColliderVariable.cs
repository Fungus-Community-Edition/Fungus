using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Collider variable type.
    /// </summary>
    [VariableInfo("Physics/ThreeD", "Collider", typeof(Collider))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ColliderVariable : VariableBase<UnityEngine.Collider>
    { }

    /// <summary>
    /// Container for a Collider variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Collider), typeof(ColliderVariable))]
    public class ColliderData : VariableData<Collider>
    {
        public ColliderData() : base(default) { }

        public ColliderData(Collider startVal) : base(startVal)
        {
        }

    }

}