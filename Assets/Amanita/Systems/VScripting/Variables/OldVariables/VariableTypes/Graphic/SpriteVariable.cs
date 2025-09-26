using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Sprite variable type.
    /// </summary>
    [VariableInfo("Graphic", "Sprite", typeof(Sprite))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class SpriteVariable : VariableBase<Sprite>
    {
    }

    /// <summary>
    /// Container for a Sprite variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Sprite), typeof(SpriteVariable))]
    public class SpriteData : VariableData<Sprite>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(SpriteVariable))]
        public IVariable<Sprite> spriteRef;

        public SpriteData() : base(default) { }
        public SpriteData(Sprite startVal = null) : base(startVal) { }

        public static implicit operator Sprite(SpriteData spriteData)
        {
            return spriteData.Value;
        }

        public override void Refresh()
        {
            varRef ??= spriteRef;
        }

    }
}