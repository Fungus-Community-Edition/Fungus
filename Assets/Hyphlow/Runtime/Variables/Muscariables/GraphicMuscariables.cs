using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [System.Serializable]
    [VariableInfo("Graphic", "String", typeof(string))]
    [MovedFrom(true, "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", 
        "StringMuscariable")]
    public class StringMuscariable : Muscariable<string>
    {
        public StringMuscariable() : base()
        {
            Value = string.Empty;
        }

        public static StringMuscariable operator +(StringMuscariable a, StringMuscariable b)
            => new StringMuscariable { Value = a.Value + b.Value };

        public static bool operator ==(StringMuscariable a, StringMuscariable b)
            => a.Value == b.Value;

        public static bool operator !=(StringMuscariable a, StringMuscariable b)
            => a.Value != b.Value;

        public override bool Equals(object obj)
        {
            var other = obj as StringMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        protected override void ApplyLegacyDataOnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(value))
            {
                _value = value;
            }
            base.ApplyLegacyDataOnAfterDeserialize();
        }

    }

    [System.Serializable]
    [VariableInfo("Graphic", "Color", typeof(Color))]
    [MovedFrom(true, "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", 
        "ColorMuscariable")]
    public class ColorMuscariable : Muscariable<Color>
    {
        public ColorMuscariable() : base() { }

        public static bool operator ==(ColorMuscariable a, ColorMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(ColorMuscariable a, ColorMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ColorMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    [System.Serializable]
    [VariableInfo("Graphic", "Sprite", typeof(Sprite))]
    [MovedFrom(true, "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", 
        "SpriteMuscariable")]
    public class SpriteMuscariable : Muscariable<Sprite>
    {
        public SpriteMuscariable() : base() { }

        public static bool operator ==(SpriteMuscariable a, SpriteMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(SpriteMuscariable a, SpriteMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SpriteMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    [System.Serializable]
    [VariableInfo("Graphic", "Texture", typeof(Texture))]
    [MovedFrom(true, "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", 
        "TextureMuscariable")]
    public class TextureMuscariable : Muscariable<Texture>
    {
        public TextureMuscariable() : base() { }

        public static bool operator ==(TextureMuscariable a, TextureMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(TextureMuscariable a, TextureMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TextureMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    [System.Serializable]
    [VariableInfo("Graphic", "Material", typeof(Material))]
    [MovedFrom(true, "AtMycelia.Hyphlow",
        "AtMycelia.Amanita.Core")]
    public class MaterialMuscariable : Muscariable<Material>
    {
        public MaterialMuscariable() : base() { }

        public static bool operator ==(MaterialMuscariable a, MaterialMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(MaterialMuscariable a, MaterialMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as MaterialMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    [System.Serializable]
    [VariableInfo("Graphic", "Animator", typeof(Animator))]
    [MovedFrom(true, "AtMycelia.Hyphlow",
        "AtMycelia.Amanita.Core")]
    public class AnimatorMuscariable : Muscariable<Animator>
    {
        public AnimatorMuscariable() : base() { }

        public static bool operator ==(AnimatorMuscariable a, AnimatorMuscariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(AnimatorMuscariable a, AnimatorMuscariable b)
        {
            if (ReferenceEquals(a, b)) return false;
            if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) return a.Value != b.Value;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as AnimatorMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }
}