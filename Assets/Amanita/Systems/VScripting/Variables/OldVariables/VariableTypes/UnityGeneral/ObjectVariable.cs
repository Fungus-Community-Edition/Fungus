using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting
{
    /// <summary>
    /// Object variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "UnityObject", typeof(UnityObj), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class ObjectVariable : VariableBase<UnityObj>
    {
    }

}
