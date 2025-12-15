using System;

namespace Amanita.SaveSys
{
    [Serializable]
    public struct AudioClipState : IEquatable<AudioClipState>
    {
        public int lorekeeperIndex;
        public string clipName;

        public readonly bool IsValid
        {
            get
            {
                // In C# 9, we can't initialize an int to be a non-zero number by default,
                // so we just check the clip name here, expecting whatever's meant to set
                // this instance's fields to set both the index and the name.
                return !string.IsNullOrEmpty(clipName);
            }
        }

        public readonly bool Equals(AudioClipState other)
        {
            bool sameIndex = lorekeeperIndex == other.lorekeeperIndex;
            bool sameName = clipName == other.clipName;
            return sameIndex && sameName;
        }

        public override readonly string ToString()
        {
            return $"AudioClipState(index {lorekeeperIndex}, clip name {clipName})";
        }
    }

}