using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// AudioSource variable type.
    /// </summary>
    [VariableInfo("Audio", "AudioSource", typeof(AudioSource))]
    [AddComponentMenu("")]
    [System.Serializable]
    public class AudioSourceVariable : VariableBase<AudioSource>
    {
    }

    /// <summary>
    /// Container for an AudioSource variable reference or constant value.
    /// </summary>
    [System.Serializable]
    [VariableData(typeof(AudioSource), typeof(AudioSourceVariable))]
    public class AudioSourceData : VariableData<AudioSource, IVariable<AudioSource>>
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(AudioSourceVariable))]
        public AudioSourceVariable audioSourceRef;
        
        public static implicit operator AudioSource(AudioSourceData audioSourceData)
        {
            return audioSourceData.Value;
        }

        public AudioSourceData() : base(default) { }
        public AudioSourceData(AudioSource startVal = null) : base(startVal) { }

        public override IVariable VarRef
        {
            get { return audioSourceRef; }
            set
            {
                if (value == null) { audioSourceRef = null; return; }

                if (value.ContentType.Equals(this.ContentType))
                {
                    audioSourceRef = value as AudioSourceVariable;
                }
                else
                {
                    string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
                    throw new System.InvalidCastException(errorMessage);
                }

            }
        }

    }
}