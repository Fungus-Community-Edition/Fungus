using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sprite variable type.
    /// </summary>
    [VariableInfo("Graphic", "Sprite", typeof(Sprite), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SpriteVariable : VariableBase<Sprite>
    {
    }

}
