using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Transform variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "Transform", typeof(Transform), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class TransformVariable : VariableBase<Transform>
    {
    }

}
