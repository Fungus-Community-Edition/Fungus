using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace AtMycelia.Myceliaudio
{
    public class TrackManager : MonoBehaviour
    {
        [SerializeField] protected TrackManager _anchor;
        [SerializeField] protected TrackGroup _trackGroup;

        public virtual void Init(TrackGroup trackGroup)
        {
            this.trackHolder = this.gameObject;
            this.Group = trackGroup;
            SetUpInitialTracks();
            this.Anchor = _anchor; // To get volumes adjusted properly
        }

        protected GameObject trackHolder;
        public virtual TrackGroup Group
        {
            get { return _trackGroup; }
            protected set { _trackGroup = value; }
        }

        protected virtual void SetUpInitialTracks()
        {
            for (int i = 0; i < initTrackCount; i++)
            {
                EnsureTrackExists(i);
            }
        }

        protected int initTrackCount = 2;

        protected virtual void EnsureTrackExists(int id)
        {
            if (!tracks.ContainsKey(id))
            {
                AudioTrack newTrack = new AudioTrack();
                newTrack.Init(trackHolder);
                newTrack.ID = id;
                newTrack.Anchor = this;
                tracks[id] = newTrack;
                _defaultFadeTweens.Add(newTrack, null);
            }
        }

        protected IDictionary<int, AudioTrack> tracks = new Dictionary<int, AudioTrack>();
        protected IDictionary<AudioTrack, IEnumerator> _defaultFadeTweens = new Dictionary<AudioTrack, IEnumerator>();

        public virtual TrackManager Anchor
        {
            get { return _anchor; }
            set
            {
                TrackManager prevAnchor = _anchor;
                if (prevAnchor != null)
                {
                    prevAnchor.RealVolumeChanged -= OnAnchorVolChanged;
                }

                _anchor = value;
                if (_anchor != null)
                {
                    _anchor.RealVolumeChanged += OnAnchorVolChanged;
                }

                RealVolumeChanged(RealVolume);
            }
        }

        public virtual void Play(IPlayAudioContext args)
        {
            EnsureTrackExists(args.Track);
            tracks[args.Track].Play(args);
        }

        public virtual void PlayOneShot(int trackID, AudioClip clip)
        {
            EnsureTrackExists(trackID);
            tracks[trackID].PlayOneShot(clip);
        }

        public virtual void SetTrackVolume(AlterAudioSourceArgs args)
        {
            EnsureTrackExists(args.Track);
            tracks[args.Track].BaseVolume = args.TargetValue;
        }

        public virtual void SetTrackVolume(float newVol, int trackToSetFor = 0)
        {
            EnsureTrackExists(trackToSetFor);
            tracks[trackToSetFor].BaseVolume = newVol;
        }

        public virtual float GetTrackBaseVolume(int trackToGetFor = 0)
        {
            EnsureTrackExists(trackToGetFor);
            return tracks[trackToGetFor].BaseVolume;
        }

        /// <summary>
        /// Normalized for AudioSources so we can use it as a multiplier. You know, since those prefer scales of 0 to 1.
        /// </summary>
        public virtual float BaseVolumeNormalized
        {
            get { return BaseVolume / AudioMath.VolumeConversion; }
        }

        /// <summary>
        /// From 0 to 100. Affects how the volume gets changed. Not scaled by the anchor.
        /// This should be what's considered when showing and changing the vol level in the UI.
        /// </summary>
        public virtual float BaseVolume
        {
            // We go for 0-100 scale here so that users can work with it more intuitively. Hence why this
            // property is public while the normalized ver is not
            get
            {
                return _baseVolume;
            }
            set
            {
                _baseVolume = Mathf.Clamp(value, AudioMath.MinVol, AudioMath.MaxVol);
                RealVolumeChanged(RealVolume);
                AudioEvents.TrackSetVolChanged(Group, _baseVolume);
            }
        }

        protected float _baseVolume = 100f;

        // This affects the actual volume values that the tracks play at.
        public virtual float RealVolume
        {
            get
            {
                float result = _baseVolume;

                if (Anchor != null)
                {
                    result *= Anchor.RealVolumeNormalized;
                }

                return result;
            }
        }

        public virtual float RealVolumeNormalized
        {
            get { return RealVolume / AudioMath.VolumeConversion; }
        }
        public virtual void OnAnchorVolChanged(float newVolScale)
        {
            RealVolumeChanged(RealVolume);
        }

        public event UnityAction<float> RealVolumeChanged = delegate { };

        public virtual void FadeTrackVolume(AlterAudioSourceArgs args)
        {
            AudioTrack toTweenFor = GetTrackEnsured(args.Track);

            bool shouldUseCustomTweener = args.CustomFader != null;
            if (shouldUseCustomTweener)
            {
                args.CustomFader(args, toTweenFor);
            }
            else
            {
                IEnumerator defaultFade = _defaultFadeTweens[toTweenFor];
                if (defaultFade != null)
                {
                    StopCoroutine(defaultFade);
                }

                defaultFade = DoBasicTween(args, toTweenFor);
                _defaultFadeTweens[toTweenFor] = defaultFade;
                StartCoroutine(defaultFade);
            }
        }

        protected virtual AudioTrack GetTrackEnsured(int track)
        {
            EnsureTrackExists(track);
            return tracks[track];
        }

        protected virtual IEnumerator DoBasicTween(AlterAudioSourceArgs args, AudioTrack toTween)
        {
            float timer = 0f, initVolume = toTween.BaseVolume,
                targVolume = args.TargetValue;

            while (timer < args.FadeDuration)
            {
                timer += Time.deltaTime;
                float howFarAlong = timer / args.FadeDuration;
                float newVol = Mathf.Lerp(initVolume, targVolume, howFarAlong);
                toTween.BaseVolume = newVol;
                args.OnUpdate(args, newVol);
                yield return null;
            }

            args.OnComplete(args);
        }

        public virtual void Stop(int track)
        {
            var trackToStop = GetTrackEnsured(track);
            trackToStop.Stop();
        }

        public virtual float GetVolume(int track)
        {
            var trackToGetVolOf = GetTrackEnsured(track);
            return trackToGetVolOf.CurrentVolumeApplied;
        }

        public virtual string Name { get; set; }

        public virtual bool IsLoopingMain(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.IsLoopingMain;
        }

        public virtual double GetLoopStartPoint(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.LoopStartPoint;
        }

        public virtual double GetLoopEndPoint(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.LoopEndPoint;
        }

        public virtual AudioClip GetClipPlayingIn(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.ClipPlaying;
        }

        public virtual AudioClip GetIntroClip(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.IntroClipAssigned;
        }

        public virtual AudioClip GetMainClip(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.MainClipAssigned;
        }

        public virtual bool GetIsPlaying(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.IsPlaying;
        }

        public virtual bool IsPlayingIntro(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.IsPlayingIntro;
        }

        public virtual bool IsPlayingMain(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.IsPlayingMain;
        }

        public virtual float GetIntroTime(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.IntroTime;
        }

        public virtual float GetMainTime(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.MainTime;
        }

        public virtual AudioClip GetIntroClipAssigned(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.IntroClipAssigned;
        }

        public virtual AudioClip GetMainClipAssigned(int track)
        {
            var trackInvolved = GetTrackEnsured(track);
            return trackInvolved.MainClipAssigned;
        }

        public virtual void Pause(int track)
        {
            var trackToPause = GetTrackEnsured(track);
            trackToPause.Pause();
        }

        public virtual void UnPause(int track)
        {
            var trackToUnPause = GetTrackEnsured(track);
            trackToUnPause.UnPause();
        }
    }
}