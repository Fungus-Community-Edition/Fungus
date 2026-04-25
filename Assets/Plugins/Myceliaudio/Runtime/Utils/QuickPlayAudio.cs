using AtMycelia.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Myceliaudio.Utils
{
    public class QuickPlayAudio : MonoBehaviour, IAudioPlayer
    {
        [SerializeField] protected AudioTiming _timing = AudioTiming.Awake;
        [SerializeField] protected PlayAudioArgsSO[] _audioPlayConfigs = new PlayAudioArgsSO[] { };
        [SerializeField] protected bool _ignoreIfAlreadyPlaying;
        [SerializeField] protected bool _random;

        public virtual AudioTiming Timing
        {
            get { return _timing; }
            set { _timing = value; }
        }

        public virtual bool Random
        {
            get { return _random; }
            set { _random = value; }
        }

        protected virtual void Awake()
        {
            foreach (var configEl in _audioPlayConfigs)
            {
                if (configEl.MainClip == null)
                {
                    Debug.LogWarning($"{configEl.name}'s Clip field is null.");
                }
            }

            RegisterOwnerName();
            PlayAudioIfTimingIs(AudioTiming.Awake);
        }

        protected virtual void RegisterOwnerName()
        {
            // For debug messages
            _ownerName = this.name;
            if (this.transform.parent != null)
            {
                _ownerName = this.transform.parent.name;
            }
        }

        protected string _ownerName;

        protected virtual void PlayAudioIfTimingIs(AudioTiming currentTiming)
        {
            bool correctTiming = (_timing & currentTiming) > 0;
            if (!correctTiming)
            {
                return;
            }

            Play();
        }

        /// <summary>
        /// Lets you make this play audio regardless of the timing this component is set to respond to.
        /// </summary>
        public virtual void Play()
        {
            // This function is for when you want to have another script play audio without needing to
            // prep its own logic for doing so. Said script can simply have a reference to an instance
            // of this one, delegating the audio-playing logic to it.
            DecideOnConfigsToUse();
            void DecideOnConfigsToUse()
            {
                _configsToUse.Clear();
                if (_random)
                {
                    _configsToUse.Add(_audioPlayConfigs.GetRandom());
                }
                else
                {
                    _configsToUse.AddRange(_audioPlayConfigs);
                }
            }

            PlayBasedOnTheConfigs();
            void PlayBasedOnTheConfigs()
            {
                foreach (var configEl in _configsToUse)
                {
                    if (ShouldIgnore(configEl))
                    {
                        continue;
                    }
                    AudioSystem.S.Play(configEl);
                }
            }
        }

        protected IList<PlayAudioArgsSO> _configsToUse = new List<PlayAudioArgsSO>();

        protected virtual bool ShouldIgnore(PlayAudioArgsSO config)
        {
            AudioClip currentlyPlaying = AudioSystem.S.GetClipPlayingAt(config.TrackGroup, config.Track);
            bool alreadyPlayingThisClip = config.MainClip == currentlyPlaying;
            bool whetherWeShould = alreadyPlayingThisClip && _ignoreIfAlreadyPlaying;
            if (whetherWeShould)
            {
                string logMessage = $"Not going to play {config.name}'s clip in {config.TrackGroup} Track {config.Track}. Reason: it was already playing there.";
                Debug.Log(logMessage);
            }

            return whetherWeShould;
        }

        protected virtual void OnEnable()
        {
            PlayAudioIfTimingIs(AudioTiming.OnEnable);
        }

        protected virtual void OnDisable()
        {
            PlayAudioIfTimingIs(AudioTiming.OnDisable);
        }

        protected virtual void OnDestroy()
        {
            PlayAudioIfTimingIs(AudioTiming.OnDestroy);
        }
    }
}