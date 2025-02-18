// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Fungus
{
    // Meant to eventually replace the old ControlAudio
    /// <summary>
    /// Plays, loops, or stops an audiosource. Any AudioSources with the same tag as the target Audio Source will automatically be stoped.
    /// </summary>
    [CommandInfo("Audio",
                 "ControlAudioNeo",
                 "[EXPERIMENTAL] Plays, loops, or stops an audiosource. Any AudioSources with the same tag as the target Audio Source will automatically be stopped. \n\nThe volume values have to be between 0 for silent and 1 for max.")]
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

        // If there's other music playing in the scene, assign it the same tag as the new music you want to play and
        // the old music will be automatically stopped.
        protected virtual void StopAudioWithSameTag()
        {
            // Don't stop audio if there's no tag assigned
            if (_audioSource.Value == null ||
                _audioSource.Value.CompareTag("Untagged"))
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
            }

            _audioSource.Value.PlayOneShot(_audioSource.Value.clip);

            if (waitUntilFinished)
            {
                StartCoroutine(WaitAndContinue());
            }
        }

        protected virtual void FadeVolumeForPlayOnce()
        {
            if (Mathf.Approximately(fadeDuration, 0f))
            {
                _audioSource.Value.volume = endVolume.Value;
                return;
            }

            // Since the tween args go by a 0-100 scale for volume
            tweenArgs.BaseValue = _audioSource.Value.volume * 100f;
            tweenArgs.TargetValue = endVolume * 100f;
            tweenArgs.HowLongToTake = fadeDuration;

            TweenManager.TweenAudioSourceVolume(tweenArgs);
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
            if (Mathf.Approximately(fadeDuration, 0f))
            {
                _audioSource.Value.volume = endVolume.Value;
                return;
            }

            tweenArgs.BaseValue = 0;
            tweenArgs.TargetValue = endVolume * 100f;
            tweenArgs.HowLongToTake = fadeDuration;

            TweenManager.TweenAudioSourceVolume(tweenArgs);
        }

        protected virtual void PauseLoop()
        {
            if (fadeDuration > 0)
            {
                FadeVolumeForPauseLoop();
            }
            else
            {
                _audioSource.Value.GetComponent<AudioSource>().Pause();
            }
        }

        protected virtual void FadeVolumeForPauseLoop()
        {
            tweenArgs.BaseValue = _audioSource.Value.volume * 100f;
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

            TweenManager.TweenAudioSourceVolume(tweenArgs);
        }

        protected virtual void StopLoop(AudioSource source)
        {
            if (fadeDuration > 0)
            {
                FadeVolumeForStopLoop(source);
            }
            else
            {
                source.GetComponent<AudioSource>().Stop();
            }
        }

        protected virtual void FadeVolumeForStopLoop(AudioSource source)
        {
            tweenArgs.BaseValue = _audioSource.Value.volume * 100f;
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

            TweenManager.TweenAudioSourceVolume(tweenArgs);
        }

        protected virtual void ChangeVolume()
        {
            FadeVolumeForChangeVolume();
        }

        protected virtual void FadeVolumeForChangeVolume()
        {
            tweenArgs.BaseValue = startVolume * 100f;
            tweenArgs.TargetValue = endVolume * 100f;
            tweenArgs.HowLongToTake = fadeDuration;
            tweenArgs.OnComplete = (AudioTweenArgs args) =>
            {
                if (waitUntilFinished)
                {
                    Continue();
                }
            };

            TweenManager.TweenAudioSourceVolume(tweenArgs);
        }

        protected virtual void AudioFinished()
        {
            if (waitUntilFinished)
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
                    fadeType = " Fade in volume to " + endVolume.Value;
                }
                if (control == ControlAudioType.ChangeVolume)
                {
                    fadeType = " to " + endVolume.Value;
                }
                fadeType += " over " + fadeDuration.Value + " seconds.";
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