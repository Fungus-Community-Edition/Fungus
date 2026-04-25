namespace AtMycelia.Myceliaudio
{
    /// <summary>
    /// Changes the volume of a Track Set when a slider's value changes.
    /// </summary>
    public class VolumeControllerSlider : AudioSliderComponent
    {
        protected override void OnSliderValChanged(float newVal)
        {
            if (this.ShouldUseSliderStep && IsDifferentStepValue(newVal))
            {
                _prevStepVal = newVal;
            }

            Apply();
        }

        protected override void Apply()
        {
            base.Apply();
            AlignTrackSetVolWithSlider(_slider.value);
        }

        protected virtual void AlignTrackSetVolWithSlider(float sliderVal)
        {
            AudioSystem.S.SetTrackGroupVol(_trackGroup, sliderVal);
        }

    }
}