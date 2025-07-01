using UnityEngine;

namespace Amanita.Myceliaudio
{
    /// <summary>
    /// Changes the value of a Slider when the volume of
    /// the specified TrackSet changes.
    /// </summary>
    public class VolumeAlignedSlider : AudioSliderComponent
    {
        protected override void InitApply()
        {
            base.InitApply();
            float vol = AudioSystem.S.GetTrackGroupVol(_trackGroup);
            SyncWith(vol);
        }

        protected virtual void SyncWith(float newVol)
        {
            float percentage = newVol / 100f;
            // The formula for lerp is:
            // lerpedValue = startValue + (range * percentage)
            // range = (endValue - startValue)
            float result = Mathf.Lerp(_slider.minValue, _slider.maxValue, percentage);

            _slider.SetValueWithoutNotify(result); // To avoid stack overflows
        }

        protected virtual void OnEnable()
        {
            AudioEvents.TrackSetVolChanged += OnVolumeChanged;
        }

        // We assume a 0-100 scale for the new value
        protected virtual void OnVolumeChanged(TrackGroup setInvolved, float newVol)
        {
            if (setInvolved == this._trackGroup)
            {
                SyncWith(newVol);
            }
        }

        protected virtual void OnDisable()
        {
            AudioEvents.TrackSetVolChanged -= OnVolumeChanged;
        }
    }
}