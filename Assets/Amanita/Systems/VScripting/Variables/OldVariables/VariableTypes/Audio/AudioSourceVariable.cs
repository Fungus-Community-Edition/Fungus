using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// AudioSource variable type.
    /// </summary>
    [VariableInfo("Audio", "AudioSource", typeof(AudioSource), false)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class AudioSourceVariable : VariableBase<AudioSource>
    {
    }

    
}