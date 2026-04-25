namespace AtMycelia.Myceliaudio
{
    /// <summary>
    /// Changes the value of a Slider when the volume of
    /// the specified TrackSet changes.
    /// </summary>
    public class VolumeAlignedSlider : AudioSliderComponent
    {
        protected override void Apply()
        {
            base.Apply();
            float vol = AudioSystem.S.GetTrackGroupVol(_trackGroup);
            SyncWith(vol);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
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

        protected override void OnDisable()
        {
            base.OnDisable();
            AudioEvents.TrackSetVolChanged -= OnVolumeChanged;
        }
    }
}