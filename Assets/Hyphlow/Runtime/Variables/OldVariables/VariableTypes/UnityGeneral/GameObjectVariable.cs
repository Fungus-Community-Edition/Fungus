using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// GameObject variable type.
    /// </summary>
    [VariableInfo("UnityGeneral", "GameObject", typeof(GameObject), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class GameObjectVariable : VariableBase<GameObject>
    {
    }

}
