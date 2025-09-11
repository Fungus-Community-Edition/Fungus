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
    public class TextureData : VariableData<Texture>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(TextureVariable))]
        public IVariable<Texture> textureRef;
        
        public TextureData() : base(default) { }

        public TextureData(Texture startVal) : base(startVal)
        {
        }

        public override void Refresh()
        {
            varRef ??= textureRef;
        }

    }
}