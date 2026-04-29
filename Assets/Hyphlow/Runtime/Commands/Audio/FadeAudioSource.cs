using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.AmaniTween.VScripting
{
    [CommandInfo("Audio",
                 "Fade Source",
                 "Fades an audio source to a target volume over a period of time.")]
    public class FadeAudioSource : BaseSimpleTweenCommand
    {
        [SerializeField] protected AudioSourceData _source;
        [Tooltip("Goes by a scale of 0 for no sound and 100 for full volume.")]
        [SerializeField] protected FloatData _targetVolume = new FloatData(50f);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_source);
            _variableDataCache.Add(_targetVolume);
        }

        protected override void RegisterAllTargets()
        {
            _targetSource = null;
            if (_source == null)
            {
                Debug.LogError("No audio source specified for FadeAudioSource command.");
                return;
            }

            _targetSource = _source.Value;
            _allTargets.Add(_targetSource);
        }

        protected override bool AreTargetsValid()
        {
            bool result = _targetSource != null && _source.Value != null;
            return result;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _ourTween?.Kill();
            _ourTween = _audioTweener.FadeVolume(_targetSource, _targetVolume.Value, _duration);
            return _ourTween;
        }

        protected IAudioSourceTweenAdapter _audioTweener;

        protected override void ValidateTweener()
        {
            base.ValidateTweener();
            _audioTweener = _tweenerSO as IAudioSourceTweenAdapter;
            if (_audioTweener == null)
            {
                Debug.LogError("The provided tweener does not implement IAudioSourceTweenAdapter." +
                    "Please provide a compatible tweener.");
            }
        }

        private AudioSource _targetSource;

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        public override string GetSummary()
        {
            string result;
            if (_source == null || _source.Value == null)
            {
                result = "Needs audio source.";
            }
            else
            {
                result = $"{_source.Value.name} to {_targetVolume}% volume over {_duration} seconds.";
            }
            return result;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            KeepTargVolWithinBounds();
        }

        protected virtual void KeepTargVolWithinBounds()
        {
            if (_targetVolume.RepresentingVar) 
            {
                // If the var's value is out of bounds, don't worry. Let's trust the user.
                return;
            }

            if (_targetVolume.Value < 0f)
            {
                _targetVolume.Value = 0f;
            }
            else if (_targetVolume.Value > 100f)
            {
                _targetVolume.Value = 100f;
            }
        }
    }
}