using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Transform variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "Transform", typeof(Transform), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class TransformVariable : VariableBase<Transform>
    {
    }

}
