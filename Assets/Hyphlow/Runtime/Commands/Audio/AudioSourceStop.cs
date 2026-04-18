using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Calls Stop on given source.
    /// </summary>
    [CommandInfo("Audio",
                 "Play Source Stop",
                     "Calls Stop on given source.")]
    [AddComponentMenu("")]
    public class AudioSourceStop : AudioSourceBase
    {
        public override void OnEnter()
        {
            _audioSource.Value.Stop();

            Continue();
        }
    }
}