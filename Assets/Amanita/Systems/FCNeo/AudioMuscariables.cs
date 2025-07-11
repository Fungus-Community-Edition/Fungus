using UnityEngine;

namespace Amanita.VScripting
{
    public class AudioClipMuscariable : Muscariable<AudioClip>
    {
        public static bool operator ==(AudioClipMuscariable a, AudioClipMuscariable b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return ReferenceEquals(a.Value, b.Value);

        }

        public static bool operator !=(AudioClipMuscariable a, AudioClipMuscariable b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;
            return !ReferenceEquals(a.Value, b.Value);

        }

        public override bool Equals(object obj)
        {
            var other = obj as AudioClipMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    public class AudioSourceMuscariable : Muscariable<AudioSource>
    {
        public static bool operator ==(AudioSourceMuscariable a, AudioSourceMuscariable b)
        {
            if (a is null || b is null) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(AudioSourceMuscariable a, AudioSourceMuscariable b)
        {
            if (a is null || b is null) return true;
            return a.Value != b.Value;
        }

        public override bool Equals(object obj)
        {
            var other = obj as AudioSourceMuscariable;
            return this == other;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }
}