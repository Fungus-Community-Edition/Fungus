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
    [MovedFrom(true, "AtMycelia.Amanita.VScripting", "AtMycelia.Amanita.Core")]
    public class ColliderVariable : VariableBase<Collider>
    { }

    

}