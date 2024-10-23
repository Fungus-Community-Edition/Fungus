// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Fungus
{
    /// <summary>
    /// Plays, loops, or stops an audiosource. Any AudioSources with the same tag as the target Audio Source will automatically be stoped.
    /// </summary>
    [CommandInfo("Audio",
                 "ControlAudioNeo",
                 "Plays, loops, or stops an audiosource. Any AudioSources with the same tag as the target Audio Source will automatically be stopped.")]
    [ExecuteInEditMode]
    public class ControlAudioNeo : Command
    {
        [Tooltip("What to do to audio")]
        [SerializeField] protected ControlAudioType control;
        public virtual ControlAudioType Control { get { return control; } }

        [Tooltip("Audio clip to play")]
        [SerializeField] protected AudioSourceData _audioSource;

        [Tooltip("Start audio at this volume")]
        [SerializeField] protected FloatData startVolume = new FloatData(1);

        [Tooltip("End audio at this volume")]
        [SerializeField] protected FloatData endVolume = new FloatData(1);

        [Tooltip("Time to fade between current volume level and target volume level.")]
        [SerializeField] protected FloatData fadeDuration;

        [Tooltip("Wait until this command has finished before executing the next command.")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(false);

        protected virtual void Awake()
        {
            tweenArgs.Target = _audioSource;
        }

        protected AudioTweenArgs tweenArgs = new AudioTweenArgs();

        // If there's other music playing in the scene, assign it the same tag as the new music you want to play and
        // the old music will be automatically stopped.
        protected virtual void StopAudioWithSameTag()
        {
            // Don't stop audio if there's no tag assigned
            if (_audioSource.Value == null ||
                _audioSource.Value.tag == "Untagged")
            {
                return;
            }

#if UNITY_6000
            var audioSources = GameObject.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
#else
            var audioSources = GameObject.FindObjectsOfType<AudioSource>();
#endif

            for (int i = 0; i < audioSources.Length; i++)
            {
                var a = audioSources[i];
                if (a != _audioSource.Value && a.tag == _audioSource.Value.tag)
                {
                    StopLoop(a);
                }
            }
        }

        protected virtual void PlayOnce()
        {
            if (fadeDuration > 0)
            {
                // Fade volume in
                FadeVolumeForPlayOnce();
                //LeanTween.value(_audioSource.Value.gameObject, 
                //    _audioSource.Value.volume, 
                //    endVolume,
                //    fadeDuration
                //).setOnUpdate(
                //    (float updateVolume)=>{
                //    _audioSource.Value.volume = updateVolume;
                //});
            }

            _audioSource.Value.PlayOneShot(_audioSource.Value.clip);

            if (waitUntilFinished)
            {
                StartCoroutine(WaitAndContinue());
            }
        }

        protected virtual AudioTweenManager Tweener { get { return AudioTweenManager.S; } }

        protected virtual void FadeVolumeForPlayOnce()
        {
            Tweener.CancelTween(_audioSource.Value, AudioTweenType.Volume);

            tweenArgs.BaseValue = _audioSource.Value.volume;
            tweenArgs.TargetValue = endVolume;
            tweenArgs.HowLongToTake = fadeDuration;

            Tweener.TweenAudioVolume(tweenArgs);
        }

        protected virtual IEnumerator WaitAndContinue()
        {
            // Poll the audiosource until playing has finished
            // This allows for things like effects added to the audiosource.
            while (_audioSource.Value.isPlaying)
            {
                yield return null;
            }

            Continue();
        }

        protected virtual void PlayLoop()
        {
            if (fadeDuration > 0)
            {
                FadeVolumeForPlayLoop();
                _audioSource.Value.loop = true;
                _audioSource.Value.GetComponent<AudioSource>().Play();
                //LeanTween.value(_audioSource.Value.gameObject,0,endVolume,fadeDuration
                //).setOnUpdate(
                //    (float updateVolume)=>{
                //    _audioSource.Value.volume = updateVolume;
                //}
                //).setOnComplete(
                //    ()=>{
                //    if (waitUntilFinished)
                //    {
                //        Continue();
                //    }
                //}
                //);
            }
            else
            {
                _audioSource.Value.volume = endVolume;
                _audioSource.Value.loop = true;
                _audioSource.Value.GetComponent<AudioSource>().Play();
            }
        }

        protected virtual void FadeVolumeForPlayLoop()
        {
            Tweener.CancelTween(_audioSource, AudioTweenType.Volume);

            tweenArgs.BaseValue = 0;
            tweenArgs.TargetValue = endVolume;
            tweenArgs.HowLongToTake = fadeDuration;

            Tweener.TweenAudioVolume(tweenArgs);
        }

        protected virtual void PauseLoop()
        {
            if (fadeDuration > 0)
            {
                FadeVolumeForPauseLoop();
                //LeanTween.value(_audioSource.Value.gameObject,_audioSource.Value.volume,0,fadeDuration
                //).setOnUpdate(
                //    (float updateVolume)=>{
                //    _audioSource.Value.volume = updateVolume;
                //}
                //).setOnComplete(
                //    ()=>{

                //    _audioSource.Value.GetComponent<AudioSource>().Pause();
                //    if (waitUntilFinished)
                //    {
                //        Continue();
                //    }
                //}
                //);
            }
            else
            {
                _audioSource.Value.GetComponent<AudioSource>().Pause();
            }
        }

        protected virtual void FadeVolumeForPauseLoop()
        {
            Tweener.CancelTween(_audioSource, AudioTweenType.Volume);

            tweenArgs.BaseValue = _audioSource.Value.volume;
            tweenArgs.TargetValue = 0;
            tweenArgs.HowLongToTake = fadeDuration;
            tweenArgs.OnComplete = (AudioTweenArgs args) =>
            {
                _audioSource.Value.GetComponent<AudioSource>().Pause();
                if (waitUntilFinished)
                {
                    Continue();
                }
            };

            Tweener.TweenAudioVolume(tweenArgs);
        }

        protected virtual void StopLoop(AudioSource source)
        {
            if (fadeDuration > 0)
            {
                FadeVolumeForStopLoop(source);
                //LeanTween.value(source.gameObject,_audioSource.Value.volume,0,fadeDuration
                //).setOnUpdate(
                //    (float updateVolume)=>{
                //    source.volume = updateVolume;
                //}
                //).setOnComplete(
                //    ()=>{

                //    source.GetComponent<AudioSource>().Stop();
                //    if (waitUntilFinished)
                //    {
                //        Continue();
                //    }
                //}
                //);
            }
            else
            {
                source.GetComponent<AudioSource>().Stop();
            }
        }

        protected virtual void FadeVolumeForStopLoop(AudioSource source)
        {
            Tweener.CancelTween(_audioSource, AudioTweenType.Volume);

            tweenArgs.BaseValue = _audioSource.Value.volume;
            tweenArgs.TargetValue = 0;
            tweenArgs.HowLongToTake = fadeDuration;
            tweenArgs.OnComplete = (AudioTweenArgs args) =>
            {
                source.GetComponent<AudioSource>().Stop();
                if (waitUntilFinished)
                {
                    Continue();
                }
            };

            Tweener.TweenAudioVolume(tweenArgs);
        }

        protected virtual void ChangeVolume()
        {
            FadeVolumeForChangeVolume();
            //LeanTween.value(_audioSource.Value.gameObject,_audioSource.Value.volume,endVolume,fadeDuration
            //).setOnUpdate(
            //    (float updateVolume)=>{
            //    _audioSource.Value.volume = updateVolume;
            //}).setOnComplete(
            //    ()=>{
            //    if (waitUntilFinished)
            //    {
            //        Continue();
            //    }
            //});
        }

        protected virtual void FadeVolumeForChangeVolume()
        {
            Tweener.CancelTween(_audioSource, AudioTweenType.Volume);

            tweenArgs.BaseValue = startVolume;
            tweenArgs.TargetValue = endVolume;
            tweenArgs.HowLongToTake = fadeDuration;
            tweenArgs.OnComplete = (AudioTweenArgs args) =>
            {
                if (waitUntilFinished)
                {
                    Continue();
                }
            };

            Tweener.TweenAudioVolume(tweenArgs);
        }

        protected virtual void AudioFinished()
        {
            if (waitUntilFinished)
            {
                Continue();
            }
        }

        #region Public members

        public override void OnEnter()
        {
            if (_audioSource.Value == null)
            {
                Continue();
                return;
            }

            if (fadeDuration <= 0)
            {
                _audioSource.Value.volume = endVolume;
            }

            switch (control)
            {
                case ControlAudioType.PlayOnce:
                    StopAudioWithSameTag();
                    PlayOnce();
                    break;
                case ControlAudioType.PlayLoop:
                    StopAudioWithSameTag();
                    PlayLoop();
                    break;
                case ControlAudioType.PauseLoop:
                    PauseLoop();
                    break;
                case ControlAudioType.StopLoop:
                    StopLoop(_audioSource.Value);
                    break;
                case ControlAudioType.ChangeVolume:
                    ChangeVolume();
                    break;
            }
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (_audioSource.Value == null)
            {
                return "Error: No sound clip selected";
            }
            string fadeType = "";
            if (fadeDuration > 0)
            {
                fadeType = " Fade out";
                if (control != ControlAudioType.StopLoop)
                {
                    fadeType = " Fade in volume to " + endVolume;
                }
                if (control == ControlAudioType.ChangeVolume)
                {
                    fadeType = " to " + endVolume;
                }
                fadeType += " over " + fadeDuration + " seconds.";
            }
            return control.ToString() + " \"" + _audioSource.Value.name + "\"" + fadeType;
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return _audioSource.audioSourceRef == variable || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector][FormerlySerializedAs("audioSource")] public AudioSource audioSourceOLD;

        protected virtual void OnEnable()
        {
            if (audioSourceOLD != null)
            {
                _audioSource.Value = audioSourceOLD;
                audioSourceOLD = null;
            }
        }

        #endregion
    }
}