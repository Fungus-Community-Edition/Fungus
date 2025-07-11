using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Calls UnPause on given source.
    /// </summary>
    [CommandInfo("Audio",
                 "Play Source UnPause",
                     "Calls UnPause on given source.")]
    [AddComponentMenu("")]
    public class AudioSourceUnPause : AudioSourceBase
    {
        public override void OnEnter()
        {
            audioSource.Value.UnPause();

            Continue();
        }
    }
}