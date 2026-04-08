using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// GameObject variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "GameObject", typeof(GameObject), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class GameObjectVariable : VariableBase<GameObject>
    {
    }

}
