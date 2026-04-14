using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// String variable type.
    /// </summary>
    [VariableInfo("Graphic", "String", typeof(string), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class StringVariable : VariableBase<string>
    {
    }

}
