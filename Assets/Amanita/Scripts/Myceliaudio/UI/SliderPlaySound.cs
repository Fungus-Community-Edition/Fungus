using UnityEngine;

namespace Amanita.Myceliaudio
{
    /// <summary>
    /// Has a sound played when a slider's value changes. The volume played at depends
    /// on the track and set thereof.
    /// </summary>
    public class SliderPlaySound : AudioSliderComponent
    {
        [SerializeField] protected AudioClip _soundToPlay;
        [SerializeField] protected int _track;
        [Tooltip("The minimum amount of time (in seconds) that must pass between sound-plays. Applies when NOT using a SliderStep component.")]
        [SerializeField] protected float _cooldownTime = 0.3f;

        protected override void Awake()
        {
            base.Awake();
            PrepAudioArgs();
        }

        protected virtual void PrepAudioArgs()
        {
            _soundPlayArgs.MainClip = _soundToPlay;
            _soundPlayArgs.TrackGroup = this._trackGroup;
            _soundPlayArgs.Track = this._track;
        }

        protected PlayAudioArgs _soundPlayArgs = new PlayAudioArgs();

        protected override void InitApply()
        {
            base.InitApply();
            PlayTheSound();
        }

        protected virtual void PlayTheSound()
        {
#if UNITY_EDITOR
            PrepAudioArgs(); // For when we want to change things in Play Mode
#endif
            AudioSystem.S.Play(_soundPlayArgs);
        }

        protected virtual void EndCooldown()
        {
            _isOnCooldown = false;
        }

        protected virtual void OnEnable()
        {
            if (ShouldUseSliderStep)
            {
                _sliderStep.StepApplied += OnSliderValueChanged;
            }
            else
            {
                _slider.onValueChanged.AddListener(OnSliderValueChanged);
            }
        }

        protected virtual void OnSliderValueChanged(float newValue)
        {
            if (this.ShouldUseSliderStep && IsDifferentStepValue(newValue))
            {
                PlayTheSound();
                _prevStepVal = newValue;
            }
            else if (!this.ShouldUseSliderStep && !_isOnCooldown)
            {
                PlayTheSound();
                _isOnCooldown = true;
                Invoke(nameof(EndCooldown), _cooldownTime);
            }
        }

        protected bool _isOnCooldown = false;

        protected virtual void OnDisable()
        {
            if (ShouldUseSliderStep)
            {
                _sliderStep.StepApplied -= OnSliderValueChanged;
            }
            else
            {
                _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }
    }
}