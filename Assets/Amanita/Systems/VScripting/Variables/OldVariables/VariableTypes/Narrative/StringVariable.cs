using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// String variable type.
    /// </summary>
    [VariableInfo("Graphic", "String", typeof(string), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class StringVariable : VariableBase<string>
    {
    }

}
