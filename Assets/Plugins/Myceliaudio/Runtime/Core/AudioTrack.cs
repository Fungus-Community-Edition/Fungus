using System.Collections;
using UnityEngine;
using AtMycelia.Audio;

namespace AtMycelia.Myceliaudio
{
    /// <summary>
    /// Helper class for that also kind of wraps Unity's built-in AudioSource component
    /// </summary>
    public class AudioTrack : IAudioTrack
    {
        public virtual int ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    GameObject.name = $"Track_{ID:D3}";
                }
            }
        }

        protected int _id;

        public virtual void Init(GameObject parent)
        {
            GameObject = new GameObject($"Track_{ID:D3}");
            GameObject.transform.SetParent(parent.transform, false);

            SetUpAudioSource();
        }

        public GameObject GameObject { get; protected set; } // Might help with custom tweens

        protected virtual void SetUpAudioSource()
        {
            _playsIntros = GameObject.AddComponent<AudioSource>();
            _playsMains = GameObject.AddComponent<AudioSource>();
            _playsIntros.playOnAwake = _playsMains.playOnAwake = false;
            _playsIntros.volume = _playsMains.volume = RealVolumeNormalized;

            _playsMains.loop = true;
            // ^Since we use this to play the loop segments of clips that have a loop start point
        }

        protected AudioSource _playsIntros, _playsMains;

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
            get { return _playsIntros.volume * AudioMath.VolumeConversion; }
            protected set
            {
                // Need to convert to a scale of 0 to 1 since that's what the base
                // audio sources prefer
                float normalizedForAudioSource = value / AudioMath.VolumeConversion;
                float withinLimits = Mathf.Clamp(normalizedForAudioSource, AudioMath.MinVol, AudioMath.MaxVolNormalized);
                _playsIntros.volume = _playsMains.volume = withinLimits;
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

        public virtual void Play(IPlayAudioContext args)
        {
            Stop();
            LoopStartPoint = args.LoopStartPoint / 1000.0;
            LoopEndPoint = args.LoopEndPoint / 1000.0;
            _playsMains.loop = args.Loop;
            _playsMains.clip = args.MainClip;

            if (args.Loop)
            {
                _playOnLoop = PlayOnLoopCoroutine(args);
                AudioSys.StartCoroutine(_playOnLoop);
            }
            else
            {
                _playsMains.Play();
            }
        }

        protected IEnumerator _playOnLoop;
        // ^Need this as a separate object to avoid loop points getting confused when
        // switching from one song to another

        protected IEnumerator PlayOnLoopCoroutine(IPlayAudioContext args)
        {
            AudioClip baseClip = args.MainClip;
            bool loopTheEntireSong = args.LoopStartPoint <= 0 && !args.HasEndPointBeforeEndOfClip;

            if (loopTheEntireSong)
            {
                // No need to do any fancy file-splitting in this case
                _playsMains.clip = baseClip;
                _playsMains.Play();
                yield break;
            }

            PlayBasedOnSplitClips();
            void PlayBasedOnSplitClips()
            {
                double loopStartPoint, loopEndPoint;
                FindLoopPoints();
                void FindLoopPoints()
                {
                    // The loop points in the args are in milliseconds, and as we need to
                    // work with points in seconds instead...
                    loopStartPoint = args.LoopStartPoint / 1000.0;
                    loopEndPoint = baseClip.PreciseLength();
                    // ^Since by default, end points are at the exact end of the audio clip
                    if (args.HasEndPointBeforeEndOfClip)
                    {
                        loopEndPoint = args.LoopEndPoint / 1000.0;
                    }
                }

                AudioClip intro = null, loopSegment;
                bool weHaveAnIntroToPlay;
                FetchSplitClips();

                void FetchSplitClips()
                {
                    weHaveAnIntroToPlay = loopStartPoint > 0 & args.HasEndPointBeforeEndOfClip;
                    // ^Can't have an intro without a loop segment. 

                    if (weHaveAnIntroToPlay)
                    {
                        intro = AudioSystem.S.GetIntroClip(baseClip, loopStartPoint);
                    }

                    loopSegment = AudioSystem.S.GetLoopClip(baseClip, loopStartPoint, loopEndPoint);
                }

                _playsIntros.clip = intro;
                _playsMains.clip = loopSegment;

                PlayTheRightClips();
                void PlayTheRightClips()
                {
                    if (weHaveAnIntroToPlay)
                    {
                        _playsIntros.Play();

                        SetLoopToPlayRightAfterIntro();
                        void SetLoopToPlayRightAfterIntro()
                        {
                            double slightBuffer = 0.00;
                            double afterTheFirstPlayIsDone = AudioSettings.dspTime +
                                _playsIntros.clip.PreciseLength() + slightBuffer;
                            _playsMains.PlayScheduled(afterTheFirstPlayIsDone);
                        }
                    }
                    else
                    {
                        _playsMains.Play();
                        // ^This would mean that the only cutoff is with the end point, and thus that
                        // the start point is at the exact beginning of the song.
                    }
                }

            }


            yield break;
        }

        protected virtual AudioSystem AudioSys { get { return AudioSystem.S; } }

        public virtual void PlayOneShot(IPlayAudioContext args)
        {
            PlayOneShot(args.MainClip);
        }

        public virtual void PlayOneShot(AudioClip clip)
        {
            _playsIntros.PlayOneShot(clip);
        }

        public virtual void Stop()
        {
            if (_playOnLoop != null)
            {
                AudioSys.StopCoroutine(_playOnLoop);
                _playOnLoop = null;
            }

            _playsIntros.Stop();
            _playsMains.Stop();
        }

        /// <summary>
        /// The clip playing on loop, as opposed to one shots
        /// </summary>
        public virtual AudioClip ClipPlaying
        {
            get
            {
                if (IsPlayingIntro)
                {
                    return _playsIntros.clip;
                }
                else if (IsPlayingMain)
                {
                    return _playsMains.clip;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Whether or not this track is playing anything
        /// </summary>
        public virtual bool IsPlaying
        {
            get { return IsPlayingIntro || IsPlayingMain; }
        }

        public virtual bool IsPlayingIntro
        {
            get { return _playsIntros.isPlaying; }
        }

        public virtual bool IsPlayingMain
        {
            get { return _playsMains.isPlaying; }
        }

        public virtual float IntroTime
        {
            get { return _playsIntros.time; }
        }
        public virtual float MainTime
        {
            get { return _playsMains.time; }
        }

        public virtual AudioClip IntroClipAssigned
        {
            get { return _playsIntros.clip; }
        }

        public virtual AudioClip MainClipAssigned
        {
            get { return _playsMains.clip; }
        }

        public virtual bool IsLoopingIntro
        {
            get { return _playsIntros.loop; }
        }

        public virtual bool IsLoopingMain
        {
            get { return _playsMains.loop; }
        }

        public virtual double LoopStartPoint { get; private set; }
        public virtual double LoopEndPoint { get; private set; }

        public virtual void Pause()
        {
            _playsIntros.Pause();
            _playsMains.Pause();
        }

        public virtual void UnPause()
        {
            _playsIntros.UnPause();
            _playsMains.UnPause();
        }
    }
}