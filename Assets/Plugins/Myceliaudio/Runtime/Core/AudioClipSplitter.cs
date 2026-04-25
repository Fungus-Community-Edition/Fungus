using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Myceliaudio
{
    /// <summary>
    /// Helper class for making seamless audio transitions. This caches the results
    /// of the splits to minimize performance impact.
    /// </summary>
    public class AudioClipSplitter
    {
        /// <summary>
        /// As in everything up to the end point in the original clip. Might want to use this
        /// instead of the intro when making the loop follow it gets a bit finicky.
        /// </summary>
        public virtual AudioClip GetForFirstPlay(AudioClip originalClip, double loopEndPoint)
        {
            bool alreadyRegistered = _firstPlayClips.ContainsKey(originalClip) &&
                _firstPlayClips[originalClip] != null;
            if (!alreadyRegistered)
            {
                AudioClip result = CopyAudioClip(originalClip, loopEndPoint);
                result.name = originalClip.name + "_FirstPlay";
                _firstPlayClips[originalClip] = result;
            }

            return _firstPlayClips[originalClip];
        }

        protected IDictionary<AudioClip, AudioClip> _firstPlayClips = new Dictionary<AudioClip, AudioClip>();

        public virtual AudioClip GetIntroClip(AudioClip originalClip, double loopStartPoint)
        {
            AudioClip result;
            bool thereIsAnIntro = loopStartPoint > 0;
            bool alreadyRegistered = _introClips.ContainsKey(originalClip) &&
                _introClips[originalClip] != null;

            if (thereIsAnIntro)
            {
                if (!alreadyRegistered)
                {
                    RegisterIntro(originalClip, loopStartPoint);
                }

                result = _introClips[originalClip];
            }
            else
            {
                result = null;
            }

            return result;
        }

        // The keys are the orig clips, the values are the split ones
        protected IDictionary<AudioClip, AudioClip> _introClips = new Dictionary<AudioClip, AudioClip>();

        protected virtual void RegisterIntro(AudioClip originalClip, double loopStartPoint)
        {
            double introLength = loopStartPoint;
            AudioClip toRegister;
            toRegister = CopyAudioClip(originalClip, introLength, 0);
            toRegister.name = originalClip.name + IntroClipNameSuffix;
            _introClips[originalClip] = toRegister;
        }

        public static string IntroClipNameSuffix { get; protected set; } = "_Intro";
        public static string LoopClipNameSuffix { get; protected set; } = "_Loop";

        protected virtual AudioClip CopyAudioClip(AudioClip originalClip,
            double resultLengthInSeconds,
            double startTimeInSeconds = 0)
        {
            if (originalClip == null)
            {
                Debug.LogError("Original AudioClip is null. Cannot create a copy.");
                return null;
            }

            // Calculate the starting sample based on the start time
            int startSample = (int)(startTimeInSeconds * originalClip.frequency);

            // Calculate the number of samples for the new clip
            int sampleCount = (int)(resultLengthInSeconds * originalClip.frequency);

            // Ensure we don't exceed the original clip's sample count
            sampleCount = Mathf.Min(sampleCount, originalClip.samples - startSample);

            if (sampleCount <= 0)
            {
                Debug.LogError("Invalid start time or clip length. No samples to copy.");
                return null;
            }

            // Create a new AudioClip with the same properties as the original
            AudioClip newClip = AudioClip.Create(
                originalClip.name + "_Copy",
                sampleCount,
                originalClip.channels,
                originalClip.frequency,
                false // Non-streaming
            );

            // Retrieve the audio data from the original clip
            float[] originalAudioData = new float[originalClip.samples * originalClip.channels];
            originalClip.GetData(originalAudioData, 0);

            // Copy only the relevant portion of the audio data
            float[] newAudioData = new float[sampleCount * originalClip.channels];
            int startIndex = startSample * originalClip.channels;
            int endIndex = startIndex + newAudioData.Length;

            for (int i = 0, j = startIndex; j < endIndex; i++, j++)
            {
                newAudioData[i] = originalAudioData[j];
            }

            // Set the data for the new clip
            newClip.SetData(newAudioData, 0);

            return newClip;
        }

        public virtual AudioClip GetLoopClip(AudioClip originalClip, double loopStartPoint, double loopEndPoint)
        {
            bool alreadyRegistered = _loopClips.ContainsKey(originalClip) &&
                _loopClips[originalClip] != null;
            if (!alreadyRegistered)
            {
                RegisterLoop(originalClip, loopStartPoint, loopEndPoint);
            }

            return _loopClips[originalClip];
        }

        // Same as with the intro clips: the keys are the origs, the values are the splits
        protected IDictionary<AudioClip, AudioClip> _loopClips = new Dictionary<AudioClip, AudioClip>();

        protected virtual void RegisterLoop(AudioClip originalClip,
            double loopStartPoint, double loopEndPoint)
        {
            double loopLength = loopEndPoint - loopStartPoint;
            AudioClip newLoop = CopyAudioClip(originalClip, loopLength, loopStartPoint);
            newLoop.name = originalClip.name + LoopClipNameSuffix;
            _loopClips[originalClip] = newLoop;
        }

        public virtual void Clear()
        {
            _firstPlayClips.Clear();
            _loopClips.Clear();
            _introClips.Clear();
        }
    }
}