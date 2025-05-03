using System.Collections;
using UnityEngine;

namespace Amanita.Myceliaudio
{
    /// <summary>
    /// Helper class for NeoNeoAudioSys that also kind of wraps Unity's built-in AudioSource component
    /// </summary>
    public class AudioTrack : IAudioTrackTweenables
    {
        public virtual int ID { get; set; }

        public virtual void Init(GameObject toWorkWith)
        {
            GameObject = toWorkWith;
            SetUpAudioSource();
        }

        public GameObject GameObject { get; protected set; } // Might help with custom tweens

        protected virtual void SetUpAudioSource()
        {
            _baseSource = GameObject.AddComponent<AudioSource>();
            _baseSource.playOnAwake = false;
            _baseSource.volume = RealVolumeNormalized;
        }

        protected AudioSource _baseSource;

        public virtual TrackManager Anchor
        {
            get { return _anchor; }
            set
            {
                TrackManager prevAnchor = _anchor;
                if (prevAnchor != null)
                {
                    prevAnchor.RealVolumeChanged += OnAnchorRealVolChanged;
                }

                _anchor = value;
                if (_anchor != null)
                {
                    _anchor.RealVolumeChanged += OnAnchorRealVolChanged;
                }

                UpdateCurrentVolApplied();
            }
        }

        protected TrackManager _anchor;

        public virtual void OnAnchorRealVolChanged(float newVolScale)
        {
            UpdateCurrentVolApplied();
        }

        protected virtual void UpdateCurrentVolApplied()
        {
            CurrentVolumeApplied = RealVolume;
        }

        /// <summary>
        /// On a scale of 0 to 100. 0 = total silence, 100 = max vol
        /// </summary>
        public virtual float CurrentVolumeApplied
        {
            get { return _baseSource.volume * AudioMath.VolumeConversion; }
            protected set
            {
                // Need to convert to a scale of 0 to 1 since that's what the base
                // audio sources prefer
                float normalizedForAudioSource = value / AudioMath.VolumeConversion;
                float withinLimits = Mathf.Clamp(normalizedForAudioSource, AudioMath.MinVol, AudioMath.MaxVolNormalized);
                _baseSource.volume = withinLimits;
            }
        }

        public virtual float RealVolume
        {
            get
            {
                float result = BaseVolume;

                if (Anchor != null)
                {
                    result *= Anchor.RealVolumeNormalized;
                }

                return result;
            }
        }

        public virtual float BaseVolume
        {
            get { return _baseVolume; }
            set
            {
                _baseVolume = value;
                UpdateCurrentVolApplied();
            }
        }

        protected float _baseVolume = 100f;

        protected virtual float RealVolumeNormalized
        {
            get { return RealVolume / AudioMath.VolumeConversion; }
        }

        public virtual void Play(PlayAudioArgs args)
        {
            if (args.Loop)
            {
                Stop();
                Loop = true; // To avoid issues for when the end point is at the exact end of the song

                _playOnLoop = PlayOnLoopCoroutine(args);
                AudioSys.StartCoroutine(_playOnLoop);
            }
            else
            {
                _baseSource.PlayOneShot(args.Clip);
            }
        }

        protected IEnumerator _playOnLoop;
        // ^Need this as a separate object to avoid loop points getting confused when
        // switching from one song to another

        protected IEnumerator PlayOnLoopCoroutine(PlayAudioArgs args)
        {
            AudioClip clip = _baseSource.clip = args.Clip;
            _baseSource.Play();

            // Since the base loop point is in milliseconds...
            float loopPoint = (float)(args.LoopStartPoint / 1000.0);
            // ^AudioSource.time is a float, not a double, so...

            double clipLength = clip.PreciseLength();
            if (args.HasLoopEndPoint)
            {
                clipLength = args.LoopEndPoint / 1000.0;
            }

            double lengthOfTheLoopSegment = clipLength - loopPoint;
            double whenToReturnToLoopPoint = AudioSettings.dspTime + clipLength;

            while (true)
            {
                if (!Loop) // Since that could get changed while we're playing
                {
                    yield break;
                }

                bool shouldReturnToLoopPoint = AudioSettings.dspTime >= whenToReturnToLoopPoint;

                if (shouldReturnToLoopPoint)
                {
                    _baseSource.time = loopPoint;
                    whenToReturnToLoopPoint += lengthOfTheLoopSegment;
                }

                yield return null;
            }
        }

        protected virtual AudioSystem AudioSys { get { return AudioSystem.S; } }

        public virtual void Stop()
        {
            if (_playOnLoop != null)
            {
                AudioSys.StopCoroutine(_playOnLoop);
                _playOnLoop = null;
            }

            _baseSource.Stop();
        }

        /// <summary>
        /// The clip playing on loop, as opposed to one shots
        /// </summary>
        public virtual AudioClip ClipPlaying
        {
            get
            {
                if (_baseSource.isPlaying)
                {
                    return _baseSource.clip;
                }
                else
                {
                    return null;
                }

            }
        }

        public virtual bool Loop
        {
            get
            {
                return _baseSource.loop;
            }
            set
            {
                _baseSource.loop = value;
            }
        }

        public virtual bool IsPlaying
        {
            get
            {
                return _baseSource.isPlaying;
            }
        }

        public virtual float Time
        {
            get
            {
                return _baseSource.time;
            }
        }
    }
}