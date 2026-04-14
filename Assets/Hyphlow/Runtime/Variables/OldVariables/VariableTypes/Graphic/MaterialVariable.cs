using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Material variable type.
    /// </summary>
    [VariableInfo("Graphic", "Material", typeof(Material), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class MaterialVariable : VariableBase<Material>
    {
    }

}
