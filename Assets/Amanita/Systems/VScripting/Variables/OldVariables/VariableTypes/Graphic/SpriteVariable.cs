using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Sprite variable type.
    /// </summary>
    [VariableInfo("Graphic", "Sprite", typeof(Sprite), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class SpriteVariable : VariableBase<Sprite>
    {
    }

}
