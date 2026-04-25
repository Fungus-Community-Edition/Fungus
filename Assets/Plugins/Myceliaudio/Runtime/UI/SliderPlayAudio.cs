using AtMycelia.Myceliaudio.Utils;
using UnityEngine;

namespace AtMycelia.Myceliaudio.UI
{
    public class SliderPlayAudio : AudioSliderComponent, IAudioPlayer
    {
        [SerializeField] protected QuickPlayAudio _audioPlayer;
        [Tooltip("The minimum amount of time (in seconds) that must pass between sound-plays. Applies when this is NOT using a SliderStep component.")]
        [SerializeField] protected float _cooldownTime = 0.3f;

        protected override void Apply()
        {
            base.Apply();
            Play();
        }

        public virtual void Play()
        {
            _audioPlayer.Play();
        }

        protected virtual void EndCooldown()
        {
            _isOnCooldown = false;
        }

        protected override void OnSliderValChanged(float newValue)
        {
            base.OnSliderValChanged(newValue);
            if (this.ShouldUseSliderStep && IsDifferentStepValue(newValue))
            {
                Play();
                _prevStepVal = newValue;
            }
            else if (!this.ShouldUseSliderStep && !_isOnCooldown)
            {
                Play();
                _isOnCooldown = true;
                Invoke(nameof(EndCooldown), _cooldownTime);
            }
        }

        protected bool _isOnCooldown = false;

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        protected virtual void OnValidate()
        {
            if (_audioPlayer == null)
            {
                _audioPlayer = gameObject.GetOrAddComponent<QuickPlayAudio>();
                _audioPlayer.Timing = AudioTiming.Null;
            }
        }
    }
}