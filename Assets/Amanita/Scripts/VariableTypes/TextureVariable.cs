


using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Texture variable type.
    /// </summary>
    [VariableInfo("Other", "Texture")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class TextureVariable : VariableBase<Texture>
    {
    }

    /// <summary>
    /// Container for a Texture variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct TextureData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TextureVariable))]
        public TextureVariable textureRef;
        
        [SerializeField]
        public Texture textureVal;

        public TextureData(Texture v)
        {
            textureVal = v;
            textureRef = null;
        }
        
        public static implicit operator Texture(TextureData textureData)
        {
            return textureData.Value;
        }

        public Texture Value
        {
            get { return (textureRef == null) ? textureVal : textureRef.Value; }
            set { if (textureRef == null) { textureVal = value; } else { textureRef.Value = value; } }
        }

        public string GetDescription()
        {
            if (textureRef == null)
            {
                return textureVal != null ? textureVal.ToString() : "Null";
            }
            else
            {
                return textureRef.Key;
            }
        }
    }
}