using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace AtMycelia.Myceliaudio
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
            result.VolumeSettings = VolumeSettings;

            return result;
        }

        private static void PrepSettings()
        {
#if !UNITY_WEBGL
            string whereTheExeIs = Application.dataPath;
            string filePath = Path.Combine(whereTheExeIs, AudioSystem.SystemSettingsFileName);
            bool fileFound = File.Exists(filePath);

            if (!fileFound)
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
#else
            systemSettings = new MyceliaudioSettings();
#endif
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

            managers.Add(masterManagerGO);
            managers.Add(bgMusicManagerGO);
            managers.Add(soundFXManagerGO);
            managers.Add(voiceManagerGO);

            return managers;
        }

    }
}