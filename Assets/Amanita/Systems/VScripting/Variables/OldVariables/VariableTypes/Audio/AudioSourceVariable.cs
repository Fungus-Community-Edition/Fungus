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
    public class AudioSourceData : VariableData<AudioSource>
    {
        [SerializeField, SerializeReference]
        [VariableProperty("<Value>", typeof(AudioSourceVariable))]
        public IVariable<AudioSource> audioSourceRef;
        
        public static implicit operator AudioSource(AudioSourceData audioSourceData)
        {
            return audioSourceData.Value;
        }

        public AudioSourceData() : base(default) { }
        public AudioSourceData(AudioSource startVal = null) : base(startVal) { }

        public override void Refresh()
        {
            varRef ??= audioSourceRef;
        }

    }
}