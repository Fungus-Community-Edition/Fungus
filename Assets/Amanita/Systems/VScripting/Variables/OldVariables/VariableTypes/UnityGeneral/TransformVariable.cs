using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Transform variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "Transform", typeof(Transform))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class TransformVariable : VariableBase<Transform>
    {
    }

}