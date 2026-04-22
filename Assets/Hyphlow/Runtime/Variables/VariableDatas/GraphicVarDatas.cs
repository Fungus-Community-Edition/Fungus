using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Container for a string variable reference or constant value.
    /// Appears as a single line property in the inspector.
    /// For a multi-line property, use StringDataMulti.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(string), typeof(IVariable<string>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class StringData : VariableData<string>, ISerializationCallbackReceiver
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(StringVariable))]
        public StringVariable stringRef;

        [SerializeField]
        [HideInInspector]
        public string stringVal;

        public StringData() : base(default)
        {
            stringVal = string.Empty;
        }

        public StringData(string startVal) : base(startVal)
        {
            stringVal = startVal;
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

        protected override string LegacyLiteralVal
        {
            get => stringVal;
            set
            {
                if (value == "") 
                {
                    // So it's easier for the backwards compatibility to know when to migrate this.
                    // It mainly checks for null, so...
                    stringVal = null;
                }
                else
                {
                    stringVal = value;
                }
            }
        }

        public override string Value
        {
            get
            {
                _backingVarRef.Refresh();
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
                    result = _value;
                }

                // To make sure we never return a null value
                if (result == null)
                {
                    result = _value = string.Empty;
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
                    base._value = value;
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

        protected override void DoBackwardsCompatibility()
        {
            if (LegacyVarRef != null)
            {
                var oldVarRef = LegacyVarRef;
                VarRef = oldVarRef;
                LegacyVarRef = null;
            }

            if (!string.IsNullOrEmpty(LegacyLiteralVal))
            {
                var oldLiteralVal = LegacyLiteralVal;
                LiteralValue = oldLiteralVal;
                LegacyLiteralVal = "";
            }
        }


    }

    [System.Serializable]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class StringDataMulti : StringData
    {
        public StringDataMulti() : base(default) { }

        public StringDataMulti(string startVal) : base(startVal)
        {
            stringVal = startVal;
        }

        public static implicit operator string(StringDataMulti strData)
        {
            return strData.Value;
        }
    }

    /// <summary>
    /// Container for a Color variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Color), typeof(IVariable<Color>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ColorData : VariableData<Color>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(ColorVariable))]
        public ColorVariable colorRef;

        [SerializeField]
        public Color colorVal;

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

        protected override Color LegacyLiteralVal
        {
            get => colorVal;
            set => colorVal = value;
        }

    }

    /// <summary>
    /// Container for a Sprite variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Sprite), typeof(IVariable<Sprite>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SpriteData : VariableData<Sprite>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(SpriteVariable))]
        public SpriteVariable spriteRef;

        [SerializeField]
        public Sprite spriteVal;

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

        protected override Sprite LegacyLiteralVal
        {
            get => spriteVal;
            set => spriteVal = value;
        }

    }

    /// <summary>
    /// Container for a Texture variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Texture), typeof(IVariable<Texture>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class TextureData : VariableData<Texture>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TextureVariable))]
        public TextureVariable textureRef;

        [SerializeField]
        public Texture textureVal;

        public TextureData() : base(default) { }

        public TextureData(Texture startVal) : base(startVal)
        {
        }

        protected override Variable LegacyVarRef
        {
            get => textureRef;
            set => textureRef = value as TextureVariable;
        }

        protected override Texture LegacyLiteralVal
        {
            get => textureVal;
            set => textureVal = value;
        }

    }

    /// <summary>
    /// Container for a Material variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(Material), typeof(IVariable<Material>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class MaterialData : VariableData<Material>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(MaterialVariable))]
        public MaterialVariable materialRef;

        [SerializeField]
        public Material materialVal;

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

        protected override Material LegacyLiteralVal
        {
            get => materialVal;
            set => materialVal = value;
        }
    }

    [System.Serializable]
    [VariableData(typeof(Animator), typeof(IVariable<Animator>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class AnimatorData : VariableData<Animator>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(AnimatorVariable))]
        public AnimatorVariable animatorRef;

        [SerializeField]
        public Animator animatorVal;

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

        protected override Animator LegacyLiteralVal
        {
            get => animatorVal;
            set => animatorVal = value;
        }
    }


}