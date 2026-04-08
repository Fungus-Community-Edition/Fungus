using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Collider variable type.
    /// </summary>
    [VariableInfo("Physics/ThreeD", "Collider", typeof(Collider), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class ColliderVariable : VariableBase<UnityEngine.Collider>
    { }

    

}