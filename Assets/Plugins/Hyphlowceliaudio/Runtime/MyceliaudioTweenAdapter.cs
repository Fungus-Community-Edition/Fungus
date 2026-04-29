using System;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using AtMycelia.AmaniTween;
using AtMycelia.Myceliaudio;

namespace AtMycelia.Hyphlowceliaudio
{
    public class MyceliaudioTweenAdapter : ScriptableObject, IMyceliaudioTweenAdapter, IAudioSourceTweenAdapter
    {
        protected virtual string GenIDFor(UnityObj unityObj, string aspectName)
        {
            string typeName = unityObj.GetType().Name;
            string result = $"{typeName}_{unityObj.name}_{unityObj.GetInstanceID()}_{aspectName}";
            return result;
        }


        public ITweenHandle FadeVolume01(IAudioTrack track, float targVal, float duration)
        {
            return FadeVolume(track, targVal * 100f, duration);
        }

        public ITweenHandle FadeVolume01(AudioSource target, float targVal, float duration)
        {
            targVal = Mathf.Clamp01(targVal);
            var tween = TweenAudioSourceVolume01(target, target.volume, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Scale of 0 to 100
        /// </summary>
        public ITweenHandle FadeVolume(IAudioTrack track, float targVal, float duration)
        {
            string id = GenIDFor(track.GameObject, "MyceliaudioVolume");
            void UpdateTheVol(float newVol)
            {
                track.BaseVolume = newVol;
            }
            Tween<float> tween = new Tween<float>(track.GameObject, id, track.BaseVolume,
                targVal, duration, UpdateTheVol);

            return DefaultTweenHandle.From(tween);
        }

        public ITweenHandle FadePitchN33(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourcePitchN33(target, target.pitch, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Uses a scale of -3 to 3. Normal pitch = 1
        /// </summary>
        public Tween<float> TweenAudioSourcePitchN33(AudioSource source, float startPitch,
            float endPitch, float duration, Action onComplete = null)
        {
            startPitch = Mathf.Clamp(startPitch, -3, 3);
            endPitch = Mathf.Clamp(endPitch, -3, 3);

            onComplete += delegate { };
            string id = GenIDFor(source, "Pitch");
            void UpdateThePitch(float val)
            {
                source.pitch = val;
            }

            Tween<float> result = new Tween<float>(source, id, startPitch, endPitch, duration, UpdateThePitch)
            .SetOnComplete(onComplete);

            return result;
        }

        public ITweenHandle ShiftPitchTo(AudioTweenArgs tweenArgs)
        {
            return FadePitch(tweenArgs.Target, tweenArgs.TargetValue, tweenArgs.HowLongToTake);
        }

        public ITweenHandle TweenAudioSourceVolume(AudioTweenArgs tweenArgs)
        {
            return FadeVolume(tweenArgs.Target, tweenArgs.TargetValue, tweenArgs.HowLongToTake);
        }

        public ITweenHandle FadePitch(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourcePitch(target, target.pitch, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Uses a scale of -300 for min to 300 for max. Normal pitch = 100
        /// </summary>
        /// <returns></returns>
        public Tween<float> TweenAudioSourcePitch(AudioSource source, float startPitch,
            float endPitch, float duration, Action onComplete = null)
        {
            return TweenAudioSourcePitchN33(source, startPitch / 100, endPitch / 100, duration, onComplete);
        }

        /// <summary>
        /// Scale of 0 to 100
        /// </summary>
        public ITweenHandle FadeVolume(AudioSource target, float targVal, float duration)
        {
            var tween = TweenAudioSourceVolume(target, target.volume, targVal, duration);
            return DefaultTweenHandle.From(tween);
        }

        /// <summary>
        /// Uses a scale of 0 to 100
        /// </summary>
        /// <returns></returns>
        public Tween<float> TweenAudioSourceVolume(AudioSource source, float startVol,
            float endVol, float duration, Action onComplete = null)
        {
            return TweenAudioSourceVolume01(source, startVol / 100, endVol / 100, duration, onComplete);
        }

        /// <summary>
        /// Uses a scale of 0 to 1
        /// </summary>
        public Tween<float> TweenAudioSourceVolume01(AudioSource source, float startVol,
            float endVol, float duration, Action onComplete = null)
        {
            startVol = Mathf.Clamp01(startVol);
            endVol = Mathf.Clamp01(endVol);

            string id = GenIDFor(source, "Volume");
            void UpdateTheVol(float newVol)
            {
                source.volume = newVol;
            }
            Tween<float> result = new Tween<float>(source, id, startVol, endVol, duration, UpdateTheVol)
                .SetOnComplete(onComplete);

            return result;
        }



    }
}