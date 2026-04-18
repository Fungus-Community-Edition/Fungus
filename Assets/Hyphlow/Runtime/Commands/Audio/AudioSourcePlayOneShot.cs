using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// PlayOneShot with given clip on given source
    /// </summary>
    [CommandInfo("Audio",
                 "Play Source One Shot",
                     "PlayOneShot with given clip on given source")]
    [AddComponentMenu("")]
    public class AudioSourcePlayOneShot : Command
    {
        [FormerlySerializedAs("audioSource")]
        [SerializeField] protected AudioSourceData _audioSource;

        [FormerlySerializedAs("audioClip")]
        [SerializeField] protected AudioClipData _audioClip;

        [Tooltip("Optional, volume scale passed into the PlayOneShot.")]
        [FormerlySerializedAs("volumeScale")]
        [SerializeField] protected FloatData _volumeScale = new FloatData(1);

        [Tooltip("Wait for the length of the clip that has been played before continuing.")]
        [FormerlySerializedAs("waitUntilFinished")]
        [SerializeField] protected bool _waitUntilFinished = false;

        public override void OnEnter()
        {
            _audioSource.Value.PlayOneShot(_audioClip.Value, _volumeScale.Value);

            if (_waitUntilFinished)
            {
                StartCoroutine(WaitForClipLength());
            }
            else
            {
                Continue();
            }
        }

        protected IEnumerator WaitForClipLength()
        {
            yield return new WaitForSeconds(_audioClip.Value.length);
            Continue();
        }

        public override string GetSummary()
        {
            if (_audioSource.Value == null)
                return "Error: no source set";

            if (_audioClip.Value == null)
                return "Error: no clip set";

            var retval = _audioSource.Value.name + ": " + _audioClip.Value.name + " @ " + _volumeScale.Value.ToString();

            if (_waitUntilFinished)
                retval += " waits";

            return retval;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        public override bool HasReference(Variable variable)
        {
            bool result = ReferenceEquals(_audioSource.VarRef, variable) ||
                ReferenceEquals(_audioClip.VarRef, variable) ||
                ReferenceEquals(_volumeScale.VarRef, variable) ||
                base.HasReference(variable);
            return result;
        }
    }
}