using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Texture variable type.
    /// </summary>
    [VariableInfo("Graphic", "Texture", typeof(Texture), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class TextureVariable : VariableBase<Texture>
    {
    }

}
