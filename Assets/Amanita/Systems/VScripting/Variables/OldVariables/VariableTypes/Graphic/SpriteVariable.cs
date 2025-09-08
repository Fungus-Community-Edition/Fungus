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
        [SerializeField]
        [VariableProperty("<Value>", typeof(SpriteVariable))]
        public SpriteVariable spriteRef;

        public SpriteData() : base(default) { }
        public SpriteData(Sprite startVal = null) : base(startVal) { }

        public static implicit operator Sprite(SpriteData spriteData)
        {
            return spriteData.Value;
        }

        public override IVariable VarRef
        {
            get { return spriteRef; }
            set
            {
                if (value == null) { spriteRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    spriteRef = value as SpriteVariable;
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }
    }
}