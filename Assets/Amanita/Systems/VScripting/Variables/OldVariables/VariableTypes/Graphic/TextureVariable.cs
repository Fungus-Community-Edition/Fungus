


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Texture variable type.
    /// </summary>
    [VariableInfo("Graphic", "Texture", typeof(Texture))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class TextureVariable : VariableBase<Texture>
    {
    }

    /// <summary>
    /// Container for a Texture variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Texture), typeof(TextureVariable))]
    public class TextureData : VariableData<Texture, IVariable<Texture>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TextureVariable))]
        public TextureVariable textureRef;
        
        public TextureData() : base(default) { }

        public TextureData(Texture startVal) : base(startVal)
        {
        }

        public override IVariable VarRef
        {
            get { return textureRef; }
            set
            {
                if (value == null) { textureRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    textureRef = value as TextureVariable;
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