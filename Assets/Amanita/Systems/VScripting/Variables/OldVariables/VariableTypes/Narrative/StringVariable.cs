using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// String variable type.
    /// </summary>
    [VariableInfo("Graphic", "String", typeof(string))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class StringVariable : VariableBase<string>
    {
    }

}