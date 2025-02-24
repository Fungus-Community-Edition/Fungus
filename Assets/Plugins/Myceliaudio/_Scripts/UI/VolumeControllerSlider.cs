namespace Myceliaudio
{
    /// <summary>
    /// Changes the volume of a Track Set when a slider's value changes.
    /// </summary>
    public class VolumeControllerSlider : AudioSliderComponent
    {
        protected override void InitApply()
        {
            base.InitApply();
            AlignTrackSetVolWithSlider(_slider.value);
        }

        protected virtual void AlignTrackSetVolWithSlider(float sliderVal)
        {
            AudioSystem.S.SetTrackGroupVol(_trackGroup, sliderVal);
        }

        protected virtual void OnEnable()
        {
            if (ShouldUseSliderStep)
            {
                _sliderStep.StepApplied += OnSliderValChanged;
            }
            else
            {
                _slider.onValueChanged.AddListener(OnSliderValChanged);
            }
        }

        protected virtual void OnSliderValChanged(float newVal)
        {
            if (this.ShouldUseSliderStep && IsDifferentStepValue(newVal))
            {
                AlignTrackSetVolWithSlider(newVal);
                _prevStepVal = newVal;
            }
            else if (!this.ShouldUseSliderStep)
            {
                AlignTrackSetVolWithSlider(newVal);
            }
        }

        protected virtual void OnDisable()
        {
            if (ShouldUseSliderStep)
            {
                _sliderStep.StepApplied -= OnSliderValChanged;
            }
            else
            {
                _slider.onValueChanged.RemoveListener(OnSliderValChanged);
            }
        }
    }
}