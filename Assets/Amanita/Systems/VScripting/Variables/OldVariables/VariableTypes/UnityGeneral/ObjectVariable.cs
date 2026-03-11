using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Object variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "UnityObject", typeof(UnityObj), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class ObjectVariable : VariableBase<UnityObj>
    {
    }

}
