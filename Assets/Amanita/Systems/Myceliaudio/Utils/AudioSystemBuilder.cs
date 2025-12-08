using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FullSerializer;
using Amanita.FSExt;

namespace Amanita.Myceliaudio
{
    public static class AudioSystemBuilder
    {
        public static AudioSystem BuildDefault()
        {
            PrepSettings();
            IList<GameObject> managers = PrepTrackManagers();
            GameObject mainSysHolder = new GameObject("Myceliaudio");

            foreach (GameObject managerEl in managers)
            {
                managerEl.transform.SetParent(mainSysHolder.transform, false);
            }

            AudioSystem result = mainSysHolder.AddComponent<AudioSystem>();

            return result;
        }

        private static void PrepSettings()
        {
#if !UNITY_WEBGL
            var filePath = Path.Combine(Application.dataPath, AudioSystem.SystemSettingsFileName);

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

        private static fsSerializer Serializer => AmanitaManager.DefaultSerializer;
        private static MyceliaudioSettings systemSettings;
        private static VolumeSettings VolumeSettings { get { return systemSettings.Volume; } }

        private static IList<GameObject> PrepTrackManagers()
        {
            IList<GameObject> managers = new List<GameObject>();

            GameObject masterManagerGO = null, bgMusicManagerGO = null,
                soundFXManagerGO = null, voiceManagerGO = null;
            PrepGameObjectsForManagers();
            void PrepGameObjectsForManagers()
            {
                masterManagerGO = new GameObject("Master");
                bgMusicManagerGO = new GameObject("BGMusic");
                soundFXManagerGO = new GameObject("SoundFX");
                voiceManagerGO = new GameObject("Voice");
            }

            TrackManager masterManager = null, bgMusicManager = null,
                soundFXManager = null, voiceManager = null;
            AddManagers();
            void AddManagers()
            {
                masterManager = masterManagerGO.AddComponent<TrackManager>();
                bgMusicManager = bgMusicManagerGO.AddComponent<TrackManager>();
                soundFXManager = soundFXManagerGO.AddComponent<TrackManager>();
                voiceManager = voiceManagerGO.AddComponent<TrackManager>();
            }

            InitManagers();
            void InitManagers()
            {
                masterManager.Init(TrackGroup.Master);
                bgMusicManager.Init(TrackGroup.BGMusic);
                soundFXManager.Init(TrackGroup.SoundFX);
                voiceManager.Init(TrackGroup.Voice);
            }

            // To make sure that things are scaled off the master volume
            bgMusicManager.Anchor = soundFXManager.Anchor = voiceManager.Anchor = masterManager;

            ApplyTheVolumes();
            void ApplyTheVolumes()
            {
                masterManager.BaseVolume = VolumeSettings.master;
                bgMusicManager.BaseVolume = VolumeSettings.bgMusic;
                soundFXManager.BaseVolume = VolumeSettings.soundFX;
                voiceManager.BaseVolume = VolumeSettings.voice;
            }

            managers.Add(masterManagerGO);
            managers.Add(bgMusicManagerGO);
            managers.Add(soundFXManagerGO);
            managers.Add(voiceManagerGO);

            return managers;
        }

    }
}