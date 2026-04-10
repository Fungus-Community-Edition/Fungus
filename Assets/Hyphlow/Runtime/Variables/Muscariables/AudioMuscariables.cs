using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [System.Serializable]
    [VariableInfo("Audio", "AudioClip", typeof(AudioClip))]
    [MovedFrom(true, "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", 
        "AudioClipMuscariable")]
    public class AudioClipMuscariable : Muscariable<AudioClip>
    {
        public AudioClipMuscariable() : base() { }

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
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    [System.Serializable]
    [VariableInfo("Audio", "AudioSource", typeof(AudioSource))]
    [MovedFrom(true, "AtMycelia.Hyphlow", 
        "AtMycelia.Amanita.Core", 
        "AudioSourceMuscariable")]
    public class AudioSourceMuscariable : Muscariable<AudioSource>
    {
        public AudioSourceMuscariable() : base() { }

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
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }
}