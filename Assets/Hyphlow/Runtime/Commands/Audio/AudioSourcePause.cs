using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Calls Pause on given source.
    /// </summary>
    [CommandInfo("Audio",
                 "Play Source Pause",
                     "Calls Pause on given source")]
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