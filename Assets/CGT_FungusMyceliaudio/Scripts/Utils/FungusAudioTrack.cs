using UnityEngine;
using Fungus;
using UnityEngine.Events;

namespace CGT.FungusExt.Myceliaudio.Internal
{
    /// <summary>
    /// Helper class for NeoAudioManager that also kind of wraps Unity's built-in AudioSource component
    /// </summary>
    public class FungusAudioTrack
    {
        protected AudioSource baseSource;

        public FungusAudioTrack(GameObject holder)
        {
            this.holder = holder;
            SetUpAudioSource();
        }

        protected GameObject holder; // So we can pull off tweens

        protected virtual void SetUpAudioSource()
        {
            baseSource = holder.AddComponent<AudioSource>();
            baseSource.playOnAwake = false;
            baseSource.volume = 0;
        }

        public virtual void Play(AudioArgs args)
        {
            baseSource.Stop();
            baseSource.clip = args.Clip;
            baseSource.loop = args.Loop;
            baseSource.Play();
        }

        protected virtual InternalAudioArgs ToInternal(AudioArgs source)
        {
            InternalAudioArgs result = InternalAudioArgs.CreateCopy(source);

            result.StartingVolume = CurrentVolume;
            result.TargetVolume = source.TargetVolume;
            result.StartingPitch = CurrentPitch;

            return result;
        }

        protected virtual void Play(InternalAudioArgs args)
        {
            //UpdateSettings(args);
            baseSource.Stop();
            baseSource.clip = args.Clip;
            baseSource.loop = args.Loop;
            baseSource.Play();
        }

        /// <returns>
        /// A version of the args' OnComplete that has the passed toExecute
        /// executing first
        /// </returns>
        protected virtual InternalAudioHandler SetBeforeOnComplete(InternalAudioArgs args,
            InternalAudioHandler toExecute)
        {
            InternalAudioHandler origOnComplete = args.OnComplete;
            InternalAudioHandler result = (InternalAudioArgs maybeOtherArgs) =>
            {
                toExecute(maybeOtherArgs);
                origOnComplete(maybeOtherArgs);
            };

            return result;
        }
        
        protected AudioClip Clip
        {
            get { return baseSource.clip; }
            set { baseSource.clip = value; }
        }

        protected bool tweeningVolume, tweeningPitch;

        protected virtual void SetVolumeWithoutDelay(AudioArgs args)
        {
            CurrentVolume = args.TargetVolume;
        }

        protected virtual void SetPitchWithoutDelay(InternalAudioArgs args)
        {
            CurrentPitch = args.TargetPitch;
        }

        protected virtual void PlayWithoutDelay(AudioArgs args)
        {
            if (args.Loop)
                baseSource.Play();
            else
                baseSource.PlayOneShot(args.Clip);
        }

        public virtual float CurrentVolume
        {
            get { return baseSource.volume; }
            protected set { baseSource.volume = value; }
        }

        public virtual float CurrentPitch
        {
            get { return baseSource.pitch; }
            protected set { baseSource.pitch = value; }
        }

        public virtual void FadeVolume(AudioArgs args)
        {
            InternalAudioArgs converted = ToInternal(args);
            FadeVolume(converted);
        }

        protected virtual void FadeVolume(InternalAudioArgs args)
        {
            float startingVolume = args.StartingVolume, targetVolume = args.TargetVolume;
            tweeningVolume = true;

            void OnFadingDone(AudioTweenArgs tweenArgs)
            {
                tweeningVolume = false;
                args.OnComplete(args);
            }

            args.VolTweenArgs.OnUpdate += TweenVolume;
            args.VolTweenArgs.OnComplete += OnFadingDone;
            
            TweenManager.TweenAudioSourceVolume(args.VolTweenArgs);
            //LeanTween.value(forTweens, startingVolume, targetVolume, args.FadeDuration)
            //    .setOnUpdate(TweenVolume)
            //    .setOnComplete(whenDoneFading);
        }

        protected AudioTweenArgs _tweenArgs;

        protected virtual void TweenVolume(float newVol)
        {
            CurrentVolume = newVol;
        }

        public virtual void SetVolume(AudioArgs args)
        {
            CurrentVolume = args.TargetVolume;
        }

        public virtual void SetPitch(AudioArgs args)
        {
            InternalAudioArgs converted = ToInternal(args);
            SetPitch(converted);
        }

        protected virtual void SetPitch(InternalAudioArgs args)
        {
            if (args.WantsFade)
            {
                FadePitch(args);
            }
            else
            {
                SetPitchWithoutDelay(args);
                args.OnComplete(args);
            }
        }

        protected virtual void FadePitch(InternalAudioArgs args)
        {
            float startingPitch = CurrentPitch, targetPitch = args.TargetPitch;
            tweeningPitch = true;

            UnityAction<AudioTweenArgs> onComplete = (AudioTweenArgs tweenArgs) =>
            {
                tweeningPitch = false;
                args.OnComplete(args);
            };

            args.PitchTweenArgs.OnUpdate += TweenPitch;
            args.PitchTweenArgs.OnComplete += onComplete;
            TweenManager.TweenAudioSourcePitch(args.PitchTweenArgs);
            //LeanTween.value(forTweens, startingPitch, targetPitch, args.FadeDuration)
            //    .setOnUpdate(TweenPitch)
            //    .setOnComplete(onComplete);
        }

        protected virtual void TweenPitch(float newPitch)
        {
            baseSource.pitch = newPitch;
        }

        protected virtual bool Loop
        {
            get { return baseSource.loop; }
            set { baseSource.loop = value; }
        }

        protected virtual float AtTime
        {
            get { return baseSource.time; }
            set { baseSource.time = value; }
        }

        public virtual void Stop(AudioArgs args)
        {
            baseSource.Stop();
            args.OnComplete(args);
        }
    
    }
}