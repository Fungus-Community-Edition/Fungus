using System.Collections;
using UnityEngine;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Play a source, optionaly setting the clip and delay when called.
    /// </summary>
    [CommandInfo("Audio",
                 "Play Source",
                     "Play a source, optionaly setting the clip and delay when called.")]
    [AddComponentMenu("")]
    public class AudioSourcePlay : Command
    {
        [SerializeField] protected AudioSourceData _audioSource;

        [Tooltip("Optional clip to set on the source before playing")]
        [SerializeField] protected AudioClipData _audioClip;

        [Tooltip("Optional, if non-zero will call PlayDelayed with delay value.")]
        [SerializeField] protected FloatData _delay = new FloatData(0);

        [Tooltip("If true, will change the target source loop to matching the given 'loop' variable below.")]
        [SerializeField] protected bool _modifySourceLooping = false;

        [Tooltip("Wait for the length of the clip that has been played before continuing.")]
        [SerializeField] protected bool _loop = false;

        [Tooltip("Wait for the length of the clip that has been played before continuing.")]
        [SerializeField] protected bool _waitUntilFinished = false;

        public override void OnEnter()
        {
            if (_audioClip.Value != null)
            {
                _audioSource.Value.clip = _audioClip.Value;
            }

            if (_delay.Value != 0)
            {
                _audioSource.Value.PlayDelayed(_delay);
            }
            else
            {
                _audioSource.Value.Play();
            }

            if(_modifySourceLooping)
            {
                _audioSource.Value.loop = _loop;
            }

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
            yield return new WaitForSeconds(_audioSource.Value.clip.length);
            Continue();
        }

        public override string GetSummary()
        {
            if (_audioSource.Value == null)
                return "Error: no source set";

            var retval = _audioSource.Value.name;

            if (_audioClip.Value != null)
                retval += ": " + _audioClip.Value.name;

            if (_delay.Value != 0)
                retval += ", " + _delay.Value.ToString() + "s";

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
            return ReferenceEquals(_audioSource.VarRef, variable) ||
                ReferenceEquals(_audioClip.audioClipRef, variable) ||
                ReferenceEquals(_delay.floatRef, variable) ||
                base.HasReference(variable);
        }
    }
}