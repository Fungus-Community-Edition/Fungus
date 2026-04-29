using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Calls UnPause on given source.
    /// </summary>
    [CommandInfo("Audio",
                 "Unpause A.Source",
                 "Calls UnPause on given AudioSource.")]
    [AddComponentMenu("")]
    public class AudioSourceUnPause : AudioSourceBase
    {
        public override void OnEnter()
        {
            _audioSource.Value.UnPause();
            Continue();
        }
    }
}