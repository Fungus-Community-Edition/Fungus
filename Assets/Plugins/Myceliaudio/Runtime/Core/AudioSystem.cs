#define MYCELIAUDIO
#define MYCELIAUDIO_1_01_01f1_OR_LATER
#define MYCELIAUDIO_1_03_01f1_OR_LATER
using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Myceliaudio
{
    public class AudioSystem : MonoBehaviour, IAudioPlayer<IPlayAudioContext>
    {
        public static AudioSystem S
        {
            get
            {
                EnsureExists();
                return _s;
            }
        }

        protected static void EnsureExists()
        {
            // We check here to avoid creating craploads of AudioSyses from lots of
            // AudioCommands being executed in short order
            bool alreadySetUp = _s != null;
            if (alreadySetUp)
            {
                return;
            }

            _s = AudioSystemBuilder.BuildDefault();
        }

        public virtual VolumeSettings VolumeSettings
        {
            get => _volumeSettings;
            set
            {
                _volumeSettings.master = Mathf.Clamp(value.master, 0, 100);
                _volumeSettings.bgMusic = Mathf.Clamp(value.bgMusic, 0, 100);
                _volumeSettings.soundFX = Mathf.Clamp(value.soundFX, 0, 100);
                _volumeSettings.voice = Mathf.Clamp(value.voice, 0, 100);
                Apply(_volumeSettings);
            }
        }

        protected VolumeSettings _volumeSettings = new VolumeSettings(50, 100, 100, 100);

        protected virtual void Awake()
        {
            if (_s != null && _s != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                _s = this;
            }

            RegisterTrackManagers();
            DontDestroyOnLoad(this.gameObject);
        }

        protected static AudioSystem _s;
        protected AudioClipSplitter _clipSplitter = new AudioClipSplitter();

        protected virtual void RegisterTrackManagers()
        {
            IList<TrackManager> managersFound = GetComponentsInChildren<TrackManager>();

            foreach (TrackManager manager in managersFound)
            {
                TrackManagers[manager.Group] = manager;
            }
        }

        public IDictionary<TrackGroup, TrackManager> TrackManagers = new Dictionary<TrackGroup, TrackManager>();

        /// <summary>
        /// Returns the real volume of the specified track, taking into account the base volume of the 
        /// track's group and any anchors it may have. This is the value that will actually be heard 
        /// when the track is played.
        /// Scale: 0-100, where 0 is silence and 100 is full volume. 
        /// </summary>
        /// <param name="trackGroup"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        public virtual float GetTrackVol(TrackGroup trackGroup, int track = 0)
        {
            TrackManager managerToUse = TrackManagers[trackGroup];
            return managerToUse.GetVolume(track);
        }

        public virtual float GetTrackBaseVol(TrackGroup trackGroup, int track = 0)
        {
            TrackManager managerToUse = TrackManagers[trackGroup];
            return managerToUse.GetTrackBaseVolume(track);
        }

        public virtual void SetTrackVol(AlterAudioSourceArgs args)
        {
            TrackManager managerToUse = TrackManagers[args.TrackGroup];
            managerToUse.SetTrackVolume(args);
        }

        public virtual void SetTrackVol(TrackGroup trackGroup, int track, float targVol)
        {
            AlterAudioSourceArgs args = new AlterAudioSourceArgs()
            {
                TrackGroup = trackGroup,
                Track = track,
                TargetValue = targVol
            };

            SetTrackVol(args);
        }

        public virtual float GetTrackGroupVol(TrackGroup trackGroup)
        {
            TrackManager managerToUse = TrackManagers[trackGroup];
            return managerToUse.BaseVolume;
        }

        public virtual void SetTrackGroupVol(TrackGroup trackGroup, float newVol)
        {
            TrackManager managerToUse = TrackManagers[trackGroup];
            managerToUse.BaseVolume = newVol;
        }

        public virtual void Play(IPlayAudioContext args)
        {
            if (args.OneShot)
            {
                PlayOneShot(args);
            }
            else
            {
                var managerToInvolve = TrackManagers[args.TrackGroup];
                managerToInvolve.Play(args);
            }
        }

        public virtual void PlayOneShot(IPlayAudioContext args)
        {
            PlayOneShot(args.TrackGroup, args.Track, args.MainClip);
        }

        public virtual void PlayOneShot(TrackGroup group, int track, AudioClip clip)
        {
            var managerToInvolve = TrackManagers[group];
            managerToInvolve.PlayOneShot(track, clip);
        }

        public virtual void StopPlaying(TrackGroup trackGroup, int track = 0)
        {
            TrackManager managerToUse = TrackManagers[trackGroup];
            managerToUse.Stop(track);
        }

        /// <summary>
        /// Fades the volume of the specified track to the target value over the specified duration.
        /// If a custom fader is provided in the args, it will be used instead of the default 
        /// fading behavior. This affects a track's Base Volume.
        public virtual void FadeTrackVol(AlterAudioSourceArgs args)
        {
            TrackManager managerToUse = TrackManagers[args.TrackGroup];
            managerToUse.FadeTrackVolume(args);
        }

        public static string SystemSettingsFileName { get; set; } = "myceliaudioSettings.json";

        public virtual AudioClip GetClipPlayingAt(TrackGroup trackGroup, int track)
        {
            var manager = TrackManagers[trackGroup];
            return manager.GetClipPlayingIn(track);
        }

        public virtual bool IsPlaying(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetIsPlaying(track);
        }

        public virtual bool IsPlayingIntro(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.IsPlayingIntro(track);
        }

        public virtual bool IsPlayingMain(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.IsPlayingMain(track);
        }

        public virtual float GetIntroTime(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetIntroTime(track);
        }

        public virtual float GetMainTime(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetMainTime(track);
        }

        public virtual AudioClip GetMainClipAssigned(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetMainClip(track);
        }

        public virtual bool IsLoopingMain(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.IsLoopingMain(track);
        }

        public virtual double GetLoopStartPoint(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetLoopStartPoint(track);
        }

        public virtual double GetLoopEndPoint(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetLoopEndPoint(track);
        }

        public virtual AudioClip GetIntroClipAssigned(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetIntroClipAssigned(track);
        }

        public virtual AudioClip GetIntroClip(AudioClip originalClip, double loopStartPoint)
        {
            return _clipSplitter.GetIntroClip(originalClip, loopStartPoint);
        }

        public virtual AudioClip GetLoopClip(AudioClip originalClip, double loopStartPoint, double loopEndPoint)
        {
            return _clipSplitter.GetLoopClip(originalClip, loopStartPoint, loopEndPoint);
        }

        protected virtual void OnDestroy()
        {
            _clipSplitter.Clear();
        }

        public virtual void Apply(VolumeSettings settings)
        {
            var masterManager = TrackManagers[TrackGroup.Master];
            masterManager.BaseVolume = settings.master;

            var bgMusicManager = TrackManagers[TrackGroup.BGMusic];
            bgMusicManager.BaseVolume = settings.bgMusic;

            var sfxManager = TrackManagers[TrackGroup.SoundFX];
            sfxManager.BaseVolume = settings.soundFX;

            var voiceManager = TrackManagers[TrackGroup.Voice];
            voiceManager.BaseVolume = settings.voice;
        }

        public virtual void Pause(TrackGroup trackGroup, int track = 0)
        {
            var manager = TrackManagers[trackGroup];
            manager.Pause(track);
        }

        public virtual void UnPause(TrackGroup trackGroup, int track = 0)
        {
            var manager = TrackManagers[trackGroup];
            manager.UnPause(track);
        }


    }
}