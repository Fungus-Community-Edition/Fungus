using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Amanita.Myceliaudio
{
    public static class AudioSystemBuilder
    {
        public static AudioSystem BuildDefault()
        {
            PrepSettingsFile();
            IList<GameObject> managers = PrepTrackManagers();
            GameObject mainSysHolder = new GameObject("Myceliaudio");

            foreach (GameObject managerEl in managers)
            {
                managerEl.transform.SetParent(mainSysHolder.transform, false);
            }

            AudioSystem result = mainSysHolder.AddComponent<AudioSystem>();

            return result;
        }

        private static void PrepSettingsFile()
        {
            var filePath = Path.Combine(Application.dataPath, AudioSystem.SystemSettingsFileName);

            if (!File.Exists(filePath))
            {
                systemSettings = new MyceliaudioSettings();
                string whatToWrite = JsonUtility.ToJson(systemSettings);
                File.WriteAllText(filePath, whatToWrite);
            }
            else
            {
                string jsonString = File.ReadAllText(filePath);
                systemSettings = JsonUtility.FromJson<MyceliaudioSettings>(jsonString);
            }
        }

        private static MyceliaudioSettings systemSettings;
        private static VolumeSettings VolumeSettings { get { return systemSettings.Volume; } }

        private static IList<GameObject> PrepTrackManagers()
        {
            IList<GameObject> managers = new List<GameObject>();

            GameObject masterManagerGO = new GameObject("Master"),
                bgMusicManagerGO = new GameObject("BGMusic"),
                soundFXManagerGO = new GameObject("SoundFX"),
                voiceManagerGO = new GameObject("Voice");

            TrackManager masterManager = masterManagerGO.AddComponent<TrackManager>(),
                bgMusicManager = bgMusicManagerGO.AddComponent<TrackManager>(),
                soundFXManager = soundFXManagerGO.AddComponent<TrackManager>(),
                voiceManager = voiceManagerGO.AddComponent<TrackManager>();

            masterManager.Init(TrackGroup.Master);
            bgMusicManager.Init(TrackGroup.BGMusic);
            soundFXManager.Init(TrackGroup.SoundFX);
            voiceManager.Init(TrackGroup.Voice);

            // To make sure that things are scaled off the master volume
            bgMusicManager.Anchor = soundFXManager.Anchor = voiceManager.Anchor = masterManager;

            masterManager.BaseVolume = VolumeSettings.master;
            bgMusicManager.BaseVolume = VolumeSettings.bgMusic;
            soundFXManager.BaseVolume = VolumeSettings.soundFX;
            voiceManager.BaseVolume = VolumeSettings.voice;

            managers.Add(masterManagerGO);
            managers.Add(bgMusicManagerGO);
            managers.Add(soundFXManagerGO);
            managers.Add(voiceManagerGO);

            return managers;
        }

    }
}