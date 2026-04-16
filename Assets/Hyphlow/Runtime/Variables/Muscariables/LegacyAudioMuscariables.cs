using UnityEngine.Audio;

namespace AtMycelia.Hyphlow
{
    [System.Serializable]
    [VariableInfo("Audio", "AudioMixer", typeof(AudioMixer))]
    public class AudioMixerMuscariable : Muscariable<AudioMixer>
    {
        public AudioMixerMuscariable() : base() { }
        public static bool operator ==(AudioMixerMuscariable a, AudioMixerMuscariable b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return ReferenceEquals(a.Value, b.Value);
        }
        public static bool operator !=(AudioMixerMuscariable a, AudioMixerMuscariable b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;
            return !ReferenceEquals(a.Value, b.Value);
        }
        public override bool Equals(object obj)
        {
            var other = obj as AudioMixerMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }
        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    [System.Serializable]
    [VariableInfo("Audio", "AudioMixerGroup", typeof(AudioMixerGroup))]
    public class AudioMixerGroupMuscariable : Muscariable<AudioMixerGroup>
    {
        public AudioMixerGroupMuscariable() : base() { }
        public static bool operator ==(AudioMixerGroupMuscariable a, AudioMixerGroupMuscariable b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return ReferenceEquals(a.Value, b.Value);
        }
        public static bool operator !=(AudioMixerGroupMuscariable a, AudioMixerGroupMuscariable b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;
            return !ReferenceEquals(a.Value, b.Value);
        }
        public override bool Equals(object obj)
        {
            var other = obj as AudioMixerGroupMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }
        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }

    [System.Serializable]
    [VariableInfo("Audio", "AudioMixerSnapshot", typeof(AudioMixerSnapshot))]
    public class AudioMixerSnapshotMuscariable : Muscariable<AudioMixerSnapshot>
    {
        public AudioMixerSnapshotMuscariable() : base() { }
        public static bool operator ==(AudioMixerSnapshotMuscariable a, AudioMixerSnapshotMuscariable b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return ReferenceEquals(a.Value, b.Value);
        }
        public static bool operator !=(AudioMixerSnapshotMuscariable a, AudioMixerSnapshotMuscariable b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;
            return !ReferenceEquals(a.Value, b.Value);
        }
        public override bool Equals(object obj)
        {
            var other = obj as AudioMixerSnapshotMuscariable;
            if (ReferenceEquals(other, null)) return false;
            return this.Value == other.Value;
        }
        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }
}