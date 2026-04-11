using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Object variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "UnityObject", typeof(UnityObj), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ObjectVariable : VariableBase<UnityObj>
    {
    }

}
