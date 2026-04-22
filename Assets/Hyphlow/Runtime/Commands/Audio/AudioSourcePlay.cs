using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("audioSource")]
        [SerializeField] protected AudioSourceData _audioSource;

        [Tooltip("Optional clip to set on the source before playing")]
        [FormerlySerializedAs("audioClip")]
        [SerializeField] protected AudioClipData _audioClip;

        [Tooltip("Optional, if non-zero will call PlayDelayed with delay value.")]
        [FormerlySerializedAs("delay")]
        [SerializeField] protected FloatData _delay = new FloatData(0);

        [Tooltip("If true, will change the target source loop to matching the given 'loop' variable below.")]
        [SerializeField] protected BooleanData _modifySourceLooping = new BooleanData(false);

        [Tooltip("Wait for the length of the clip that has been played before continuing.")]
        [SerializeField] protected BooleanData _loop = new BooleanData(false);

        [Tooltip("Wait for the length of the clip that has been played before continuing.")]
        [SerializeField] protected BooleanData _waitUntilFinished = new BooleanData(false);

        [SerializeField] protected BooleanData _ignoreIfAlreadyPlaying = new BooleanData(true);

        public override void OnEnter()
        {
            bool alreadyPlaying = _audioSource.Value.isPlaying && _audioSource.Value.clip == _audioClip.Value;
            if (alreadyPlaying && _ignoreIfAlreadyPlaying)
            {
                Continue();
                return;
            }

            if (_audioClip.Value == null)
            {
                string errorMessage = $"AudioSourcePlay command error: no clip set for source '{_audioSource.Value.name}'";
                Debug.LogError(errorMessage, this);
                Continue();
                return;
            }

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

            if (_modifySourceLooping)
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

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldModifySourceLooping)
            {
                _modifySourceLooping.Value = _oldModifySourceLooping;
                _oldModifySourceLooping = false;
            }

            if (_oldLoop)
            {
                _loop.Value = _oldLoop;
                _oldLoop = false;
            }

            if (_oldWaitUntilFinished)
            {
                _waitUntilFinished.Value = _oldWaitUntilFinished;
                _oldWaitUntilFinished = false;
            }

        }

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("modifySourceLooping")]
        protected bool _oldModifySourceLooping = false;

        [FormerlySerializedAs("loop")]
        [HideInInspector]
        [SerializeField]
        protected bool _oldLoop = false;

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("waitUntilFinished")]
        protected bool _oldWaitUntilFinished = false;
    }
}