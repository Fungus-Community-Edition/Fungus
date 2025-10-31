using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Container for a string variable reference or constant value.
    /// Appears as a single line property in the inspector.
    /// For a multi-line property, use StringDataMulti.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(string), typeof(IVariable<string>))]
    public class StringData : VariableData<string>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(StringVariable))]
        public StringVariable stringRef;

        public StringData() : base(default) { }

        public StringData(string startVal) : base(startVal)
        {
        }

        public static implicit operator string(StringData spriteData)
        {
            return spriteData.Value;
        }

        public override void Refresh()
        {
            varRef ??= stringRef;
        }

        public override string Value
        {
            get
            {
                string result;
                if (VarRef != null)
                {
                    result = (string)VarRef.BoxedValue;
                }
                else
                {
                    result = value;
                }

                // To make sure we never return a null value
                if (result == null)
                {
                    result = "";
                    if (VarRef != null)
                    {
                        VarRef.BoxedValue = result;
                    }
                    value = result;
                }

                return result;
            }
            set
            {
                if (VarRef != null)
                {
                    VarRef.BoxedValue = value;
                }
                else
                {
                    base.Value = value;
                    base.value = value;
                }
            }
        }
    }

    /// <summary>
    /// Container for a string variable reference or constant value.
    /// Appears as a multi-line property in the inspector.
    /// For a single-line property, use StringData.
    /// </summary>
    [System.Serializable]
    public class StringDataMulti : StringData
    {
        public StringDataMulti() : base(default) { }

        public StringDataMulti(string startVal) : base(startVal)
        {
        }

        public static implicit operator string(StringDataMulti spriteData)
        {
            return spriteData.Value;
        }

    }

    /// <summary>
    /// Container for a Color variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Color), typeof(IVariable<Color>))]
    public class ColorData : VariableData<Color>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(ColorVariable))]
        public ColorVariable colorRef;

        public ColorData() : base(default) { }
        public ColorData(Color startVal = default) : base(startVal) { }

        public static implicit operator Color(ColorData colorData)
        {
            return colorData.Value;
        }

        public override void Refresh()
        {
            varRef ??= colorRef;
        }

    }

    /// <summary>
    /// Container for a Sprite variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Sprite), typeof(IVariable<Sprite>))]
    public class SpriteData : VariableData<Sprite>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(SpriteVariable))]
        public SpriteVariable spriteRef;

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

    /// <summary>
    /// Container for a Texture variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Texture), typeof(IVariable<Texture>))]
    public class TextureData : VariableData<Texture>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(TextureVariable))]
        public TextureVariable textureRef;

        public TextureData() : base(default) { }

        public TextureData(Texture startVal) : base(startVal)
        {
        }

        public override void Refresh()
        {
            varRef ??= textureRef;
        }

    }

    /// <summary>
    /// Container for a Material variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Material), typeof(IVariable<Material>))]
    public class MaterialData : VariableData<Material>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(MaterialVariable))]
        public MaterialVariable materialRef;

        public MaterialData() : base(default) { }
        public MaterialData(Material startVal = null) : base(startVal) { }

        public static implicit operator Material(MaterialData materialData)
        {
            return materialData.Value;
        }

        public override void Refresh()
        {
            varRef ??= materialRef;
        }
    }

    [System.Serializable]
    [VariableData(typeof(Animator), typeof(IVariable<Animator>))]
    public class AnimatorData : VariableData<Animator>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(AnimatorVariable))]
        public AnimatorVariable animatorRef;

        public static implicit operator Animator(AnimatorData animatorData)
        {
            return animatorData.Value;
        }

        public AnimatorData() : base(default) { }
        public AnimatorData(Animator startVal = default) : base(startVal) { }

        public override void Refresh()
        {
            varRef ??= animatorRef;
        }
    }


}