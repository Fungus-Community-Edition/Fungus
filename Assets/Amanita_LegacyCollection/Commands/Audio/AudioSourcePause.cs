using UnityEngine;

namespace Amanita.VScripting
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
            audioSource.Value.Pause();

            Continue();
        }
    }
}