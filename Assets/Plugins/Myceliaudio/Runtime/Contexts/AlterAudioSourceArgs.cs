using System;
using UnityEngine.Events;

namespace AtMycelia.Myceliaudio
{
    /// <summary>
    /// Args for how you want to change some numeric aspect of an AudioSource
    /// (such as volume or pitch).
    /// </summary>
    public class AlterAudioSourceArgs : EventArgs
    {
        public virtual float TargetValue { get; set; }
        public virtual TrackGroup TrackGroup { get; set; }
        public virtual int Track { get; set; }

        public virtual bool ForFading
        {
            get { return FadeDuration > 0; }
        }

        /// <summary>
        /// If you want the value to be changed immediately, leave this at 0
        /// </summary>
        public virtual float FadeDuration { get; set; } = 0f;

        /// <summary>
        /// Executes after each change of the relevant value involved in the fade. If you're using a custom
        /// fader, this will be ignored by Myceliaudio's built-in systems; you'll need your custom
        /// fader to set up responses to each value-adjustment.
        /// </summary>
        public virtual UnityAction<AlterAudioSourceArgs, float> OnUpdate { get; set; } = delegate { };

        /// <summary>
        /// Executes when the fade is done. If you're using a custom fader, this will be ignored by
        /// Myceliaudio's built-in systems; you'll need your custom fader to set up responses
        /// to when the job's done.
        /// </summary>
        public virtual UnityAction<AlterAudioSourceArgs> OnComplete { get; set; } = delegate { };

        /// <summary>
        /// For when you don't want the fade to be pulled off with the default algorithm. Good for
        /// when you want to use something like DoTween to handle the fading.
        /// </summary>
        public virtual VolumeFadeHandler CustomFader { get; set; }

        public static AlterAudioSourceArgs CreateCopy(AlterAudioSourceArgs other)
        {
            AlterAudioSourceArgs result = new AlterAudioSourceArgs()
            {
                TargetValue = other.TargetValue,
                TrackGroup = other.TrackGroup,
                Track = other.Track,
                FadeDuration = other.FadeDuration,
                OnComplete = other.OnComplete,
                CustomFader = other.CustomFader,
            };

            return result;

        }
    }
}