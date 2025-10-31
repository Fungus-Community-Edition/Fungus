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

    
}