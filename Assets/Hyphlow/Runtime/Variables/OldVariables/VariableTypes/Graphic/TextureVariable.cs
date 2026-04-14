using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Texture variable type.
    /// </summary>
    [VariableInfo("Graphic", "Texture", typeof(Texture), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class TextureVariable : VariableBase<Texture>
    {
    }

}
