using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Events;

namespace Fungus
{
    public class AudioTweenManager : MonoBehaviour
    {
        public static AudioTweenManager S
        {
            get
            {
                bool alreadyExists = _s != null;
                if (alreadyExists)
                {
                    return _s;
                }

                GameObject managerGO = new GameObject("FungusAudioTweenManager");
                _s = managerGO.AddComponent<AudioTweenManager>();
                return _s;
            }
        }

        protected static AudioTweenManager _s;

        protected virtual void Awake()
        {
            IDictionary<AudioSource, IEnumerator> volumeTweens = new Dictionary<AudioSource, IEnumerator>();
            IDictionary<AudioSource, IEnumerator> pitchTweens = new Dictionary<AudioSource, IEnumerator>();

            TweenHolders[AudioTweenType.Volume] = volumeTweens;
            TweenHolders[AudioTweenType.Pitch] = pitchTweens;
        }

        protected virtual IDictionary<AudioTweenType, IDictionary<AudioSource, IEnumerator>> TweenHolders
        { get; set; } = new Dictionary<AudioTweenType, IDictionary<AudioSource, IEnumerator>>();

        protected WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        public virtual void Register(AudioSource sourceToRegister)
        {
            // If any of the tween-holders have the source registered, so do the other containers
            // in this class
            if (!sourcesRegistered.Contains(sourceToRegister))
            {
                foreach (var holder in TweenHolders.Values)
                {
                    holder.Add(sourceToRegister, null);
                }

                sourcesRegistered.Add(sourceToRegister);
            }
        }

        protected IList<AudioSource> sourcesRegistered = new List<AudioSource>();

        public virtual void Unregister(AudioSource sourceToUnregister)
        {
            foreach (var holder in TweenHolders.Values)
            {
                holder.Remove(sourceToUnregister);
            }
            sourcesRegistered.Remove(sourceToUnregister);
        }

        public virtual void CancelTween(AudioSource toCancelOn, AudioTweenType tweenType)
        {
            if (AutoRegister)
            {
                Register(toCancelOn);
            }
            else if (!sourcesRegistered.Contains(toCancelOn))
            {
                Debug.LogWarning($"Can't cancel a tween on {toCancelOn.name} since it wasn't registered with the AudioTweenManager. Note that its AutoRegister field is set to {AutoRegister}.");
                return;
            }

            var holder = TweenHolders[tweenType];
            IEnumerator tweenToCancel = holder[toCancelOn];
            if (tweenToCancel != null)
            {
                StopCoroutine(tweenToCancel);
            }
        }

        /// <summary>
        /// Whether or not this automatically registers audio sources
        /// that this is asked to tween
        /// </summary>
        public virtual bool AutoRegister { get; set; } = true;

        public virtual void TweenAudioVolume(AudioTweenArgs args)
        {
            AudioSource tweenTarget = args.Target;

            if (AutoRegister)
            {
                Register(tweenTarget);
            }

            CancelTween(tweenTarget, AudioTweenType.Volume);
            IEnumerator process;
            CreateTweenAudioVolumeProcess(args, out process);
            var volumeTweenHolder = TweenHolders[AudioTweenType.Volume];
            volumeTweenHolder[tweenTarget] = process;

            StartCoroutine(process);
        }

        /// <summary>
        /// The out parameter is so you can control when the tween starts.
        /// </summary>
        public virtual void CreateTweenAudioVolumeProcess(AudioTweenArgs args, out IEnumerator process)
        {
            AudioSource tweenTarget = args.Target;

            if (AutoRegister)
            {
                Register(tweenTarget);
            }

            CancelTween(tweenTarget, AudioTweenType.Volume);
            process = GeneralTweenProcess<float>(args, FloatLerper, AudioSourceVolPreUpdate);
            var volumeTweenHolder = TweenHolders[AudioTweenType.Volume];
            volumeTweenHolder[tweenTarget] = process;
        }

        protected virtual IEnumerator GeneralTweenProcess<TTargetVal>(TweenArgs args,
            LerpFunc<TTargetVal> lerper,
            UnityAction<TweenArgs, TTargetVal> preOnUpdate)
        {
            TTargetVal baseVal = (TTargetVal)args.BaseValue, targetVal = (TTargetVal)args.TargetValue;
            float timer = 0, howLongToTake = args.HowLongToTake;

            while (timer < howLongToTake)
            {
                timer += Time.deltaTime;
                float howFarAlong = Mathf.Clamp(timer / howLongToTake, 0, 1);
                // ^We don't want to overshoot things when we get to the end
                TTargetVal newVal = lerper(baseVal, targetVal, howFarAlong);
                preOnUpdate(args, newVal);
                args.OnUpdate(newVal);
                yield return waitForEndOfFrame;
            }

            args.OnUpdate(args);

        }

        protected virtual float FloatLerper(float baseVal, float targetVal, float howFarAlong)
        {
            float result = baseVal + (targetVal - baseVal) * howFarAlong;
            return result;
        }

        protected virtual void AudioSourceVolPreUpdate(TweenArgs args, float newVol)
        {
            AudioTweenArgs audioArgs = args as AudioTweenArgs;
            audioArgs.Target.volume = newVol;
        }

        public virtual void TweenAudioPitch(AudioTweenArgs args)
        {
            AudioSource tweenTarget = args.Target;

            if (AutoRegister)
            {
                Register(tweenTarget);
            }

            CancelTween(tweenTarget, AudioTweenType.Pitch);
            IEnumerator process;
            CreateTweenAudioPitchProcess(args, out process);
            var pitchTweenHolder = TweenHolders[AudioTweenType.Pitch];
            pitchTweenHolder[tweenTarget] = process;
            StartCoroutine(process);
        }

        public virtual void CreateTweenAudioPitchProcess(AudioTweenArgs args, out IEnumerator process)
        {
            AudioSource tweenTarget = args.Target;

            if (AutoRegister)
            {
                Register(tweenTarget);
            }

            CancelTween(tweenTarget, AudioTweenType.Pitch);
            process = TweenAudioPitchProcess(args);
            var pitchTweenHolder = TweenHolders[AudioTweenType.Pitch];
            pitchTweenHolder[tweenTarget] = process;
        }

        protected virtual IEnumerator TweenAudioPitchProcess(AudioTweenArgs args)
        {
            AudioSource source = args.Target;
            float basePitch = args.BaseValue, targetPitch = args.TargetValue,
                timer = 0, howLongToTake = args.HowLongToTake;

            while (timer < howLongToTake)
            {
                timer += Time.deltaTime;
                float howFarAlong = timer / howLongToTake;
                float newPitch = Mathf.Lerp(basePitch, targetPitch, howFarAlong);
                source.pitch = newPitch;
                args.OnUpdate(newPitch);
                yield return waitForEndOfFrame;
            }

            args.OnComplete(args);
        }
    }

    public enum AudioTweenType
    {
        Null,
        Volume, Pitch
    }
}