using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Sprite variable type.
    /// </summary>
    [VariableInfo("Graphic", "Sprite", typeof(Sprite), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
    public class SpriteVariable : VariableBase<Sprite>
    {
    }

}
