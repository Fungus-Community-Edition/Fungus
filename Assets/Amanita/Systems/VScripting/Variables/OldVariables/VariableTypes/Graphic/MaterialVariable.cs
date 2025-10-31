using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Material variable type.
    /// </summary>
    [VariableInfo("Graphic", "Material", typeof(Material))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class MaterialVariable : VariableBase<Material>
    {
    }

}