using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Collider variable type.
    /// </summary>
    [VariableInfo("Physics/ThreeD", "Collider", typeof(Collider), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ColliderVariable : VariableBase<Collider>
    { }

    

}