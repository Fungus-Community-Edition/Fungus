using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Calls Pause on given source.
    /// </summary>
    [CommandInfo("Audio",
                 "Pause A.Source",
                "Calls Pause on given AudioSource")]
    [AddComponentMenu("")]
    public class AudioSourcePause : AudioSourceBase
    {
        public override void OnEnter()
        {
            _audioSource.Value.Pause();

            Continue();
        }
    }
}