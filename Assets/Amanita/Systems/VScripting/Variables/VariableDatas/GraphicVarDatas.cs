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
        [SerializeField]
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

        protected override Variable LegacyVarRef
        {
            get => stringRef;
            set => stringRef = value as StringVariable;
        }

        public override string Value
        {
            get
            {
                backingVarRef.Refresh();
                string result;
                if (VarRef != null)
                {
                    if (VarRef.BoxedValue is not string)
                    {
                        Debug.LogError($"StringData: Variable reference does not contain a string value.");
                    }
                    result = (string)VarRef.BoxedValue;
                }
                else
                {
                    result = value;
                }

                // To make sure we never return a null value
                if (result == null)
                {
                    result = value = string.Empty;
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

        public override string GetDescription()
        {
            if (stringRef != null)
            {
                return $"{stringRef.Key}";
            }
            else
            {
                return $"\"{Value}\"";
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
        [SerializeField]
        [VariableProperty("<Value>", typeof(ColorVariable))]
        public ColorVariable colorRef;

        public ColorData() : base(default) { }
        public ColorData(Color startVal = default) : base(startVal) { }

        public static implicit operator Color(ColorData colorData)
        {
            return colorData.Value;
        }

        protected override Variable LegacyVarRef
        {
            get => colorRef;
            set => colorRef = value as ColorVariable;
        }

    }

    /// <summary>
    /// Container for a Sprite variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Sprite), typeof(IVariable<Sprite>))]
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

        protected override Variable LegacyVarRef
        {
            get => spriteRef;
            set => spriteRef = value as SpriteVariable;
        }

    }

    /// <summary>
    /// Container for a Texture variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Texture), typeof(IVariable<Texture>))]
    public class TextureData : VariableData<Texture>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TextureVariable))]
        public TextureVariable textureRef;

        public TextureData() : base(default) { }

        public TextureData(Texture startVal) : base(startVal)
        {
        }

        protected override Variable LegacyVarRef
        {
            get => textureRef;
            set => textureRef = value as TextureVariable;
        }

    }

    /// <summary>
    /// Container for a Material variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Material), typeof(IVariable<Material>))]
    public class MaterialData : VariableData<Material>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(MaterialVariable))]
        public MaterialVariable materialRef;

        public MaterialData() : base(default) { }
        public MaterialData(Material startVal = null) : base(startVal) { }

        public static implicit operator Material(MaterialData materialData)
        {
            return materialData.Value;
        }

        protected override Variable LegacyVarRef
        {
            get => materialRef;
            set => materialRef = value as MaterialVariable;
        }
    }

    [System.Serializable]
    [VariableData(typeof(Animator), typeof(IVariable<Animator>))]
    public class AnimatorData : VariableData<Animator>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(AnimatorVariable))]
        public AnimatorVariable animatorRef;

        public static implicit operator Animator(AnimatorData animatorData)
        {
            return animatorData.Value;
        }

        public AnimatorData() : base(default) { }
        public AnimatorData(Animator startVal = default) : base(startVal) { }

        protected override Variable LegacyVarRef
        {
            get => animatorRef;
            set => animatorRef = value as AnimatorVariable;
        }
    }


}