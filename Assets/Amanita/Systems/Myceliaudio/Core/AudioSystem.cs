#define MYCELIAUDIO
#define AMANITA_MYCELIAUDIO
using FullSerializer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Amanita.FSExt;

namespace Amanita.Myceliaudio
{
    public class AudioSystem : MonoBehaviour, IAudioPlayer<IPlayAudioContext>
    {
        public static AudioSystem S
        {
            get => _s;
            set => _s = value;
        }

        public virtual void Init()
        {
            if (IsFullyInitted)
            {
                return;
            }

            if (_s != null && _s != this)
            {
                // With how this should be part of the AmanitaManager prefab, we'll let
                // AmanitaManager handle destroying this when appropriate
                return;
            }
            else
            {
                _s = this;
            }

            PrepSettings();
            void PrepSettings()
            {
#if !UNITY_WEBGL
                // Since we can't work with directories in WebGL...
                var filePath = Path.Combine(Application.dataPath, SystemSettingsFileName);

                if (!File.Exists(filePath))
                {
                    systemSettings = new MyceliaudioSettings();
                    string whatToWrite = Serializer.ToJson(systemSettings);
                    File.WriteAllText(filePath, whatToWrite);
                }
                else
                {
                    string jsonString = File.ReadAllText(filePath);
                    systemSettings = Serializer.FromJson<MyceliaudioSettings>(jsonString);
                }
#else
            systemSettings = new MyceliaudioSettings();
#endif
            }

            IList<TrackManager> managersFound = GetComponentsInChildren<TrackManager>();

            PrepManagers();
            void PrepManagers()
            {
                var masterManager = managersFound.Where((elem) => elem.Group == TrackGroup.Master).FirstOrDefault();
                var bgMusicManager = managersFound.Where((elem) => elem.Group == TrackGroup.BGMusic).FirstOrDefault();
                var soundFXManager = managersFound.Where((elem) => elem.Group== TrackGroup.SoundFX).FirstOrDefault();
                var voiceManager = managersFound.Where((elem) => elem.Group == TrackGroup.Voice).FirstOrDefault();

                for (int i = 0; i < managersFound.Count; i++)
                {
                    var managerEl = managersFound[i];
                    managerEl.Init(managerEl.Group);
                    // ^Since we assume this is in the AmanitaManager prefab, the manager
                    // should already be set up with its intended group
                }

                ApplyTheVolumes();
                void ApplyTheVolumes()
                {
                    masterManager.BaseVolume = VolumeSettings.master;
                    bgMusicManager.BaseVolume = VolumeSettings.bgMusic;
                    soundFXManager.BaseVolume = VolumeSettings.soundFX;
                    voiceManager.BaseVolume = VolumeSettings.voice;
                }
            }
            RegisterTrackManagers();
            void RegisterTrackManagers()
            {
                foreach (TrackManager managerEl in managersFound)
                {
                    TrackManagers[managerEl.Group] = managerEl;
                }
            }
            IsFullyInitted = true;
        }

        public virtual bool IsFullyInitted { get; protected set; } = false;
        protected fsSerializer Serializer => AmanitaManager.DefaultSerializer;

        protected static AudioSystem _s;
        private static MyceliaudioSettings systemSettings;
        private static VolumeSettings VolumeSettings { get { return systemSettings.Volume; } }
        protected AudioClipSplitter _clipSplitter = new AudioClipSplitter();

        public IDictionary<TrackGroup, TrackManager> TrackManagers = new Dictionary<TrackGroup, TrackManager>();

        public virtual float GetTrackVol(TrackGroup trackGroup, int track = 0)
        {
            TrackManager managerToUse = TrackManagers[trackGroup];
            return managerToUse.GetVolume(track);
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

        public virtual bool GetIsPlaying(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetIsPlaying(track);
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

        public virtual bool IsLoopingMain(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.IsLoopingMain(track);
        }

        public virtual void Pause(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            manager.Pause(track);
        }

        public virtual void Unpause(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            manager.Unpause(track);
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

        public virtual void OnDestroy()
        {
            if (S == this)
            {
                S = null;
            }
            _clipSplitter.Clear();
        }

        public virtual void Apply(VolumeSettings volumeSettings)
        {
            if (volumeSettings == null)
            {
                Debug.LogWarning("Volume settings are null.");
                return;
            }

            float masterVol = volumeSettings.master;
            float bgMusicVol = volumeSettings.bgMusic;
            float soundFXVol = volumeSettings.soundFX;
            float voiceVol = volumeSettings.voice;

            SetTrackGroupVol(TrackGroup.Master, masterVol);
            SetTrackGroupVol(TrackGroup.BGMusic, bgMusicVol);
            SetTrackGroupVol(TrackGroup.SoundFX, soundFXVol);
            SetTrackGroupVol(TrackGroup.Voice, voiceVol);
        }

        public virtual VolumeSettings GetVolumeSettings()
        {
            VolumeSettings volumeSettings = new VolumeSettings()
            {
                master = GetTrackGroupVol(TrackGroup.Master),
                bgMusic = GetTrackGroupVol(TrackGroup.BGMusic),
                soundFX = GetTrackGroupVol(TrackGroup.SoundFX),
                voice = GetTrackGroupVol(TrackGroup.Voice)
            };
            return volumeSettings;
        }

        public virtual AudioClip GetBaseMainClip(TrackGroup group, int track)
        {
            var manager = TrackManagers[group];
            return manager.GetBaseMainClip(track);
        }

        public static void ResetStaticsForTest()
        {
            S = null;
        }


    }
}