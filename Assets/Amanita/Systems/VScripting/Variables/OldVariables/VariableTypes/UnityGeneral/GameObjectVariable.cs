using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// GameObject variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "GameObject", typeof(GameObject), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class GameObjectVariable : VariableBase<GameObject>
    {
    }

}
