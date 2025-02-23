using UnityEngine;
using System.Collections.Generic;
using CGT.FungusExt.Myceliaudio.Internal;

namespace CGT.FungusExt.Myceliaudio
{
    /// <summary>
    /// Alternative to Fungus's built-in MusicManager that works with commands that alter
    /// music or sfx properties separately.
    /// </summary>
    public class AudioSys : MonoBehaviour
    {
        public static void EnsureExists()
        {
            // We check here to avoid creating craploads of AudioSyses from lots of
            // AudioCommands being executed in short order
            bool alreadySetUp = S != null;
            if (alreadySetUp)
            {
                return;
            }

            GameObject sysGO = new GameObject(sysName);
            AudioSys theSys = sysGO.AddComponent<AudioSys>();
            S = theSys;
        }

        protected static string sysName = "CGT_FungusAudioSystem";

        public static AudioSys S { get; protected set; }

        protected virtual void Awake()
        {
            EnsureAudioManagersAreThere();
            DontDestroyOnLoad(this.gameObject);
        }

        protected virtual void EnsureAudioManagersAreThere()
        {
            string musicManagerName = "MyceliaudioMusicManager";
            EnsureThereIsManagerFor(AudioType.Music, musicManagerName);
           
            string sfxManagerName = "MyceliaudioSFXManager";
            EnsureThereIsManagerFor(AudioType.SFX, sfxManagerName);

            string voiceManagerName = "MyceliaudioVoiceManager";
            EnsureThereIsManagerFor(AudioType.Voice, voiceManagerName);

            musicManager = audioManagers[AudioType.Music];
            sfxManager = audioManagers[AudioType.SFX];
            voiceManager = audioManagers[AudioType.Voice];
        }

        protected virtual void EnsureThereIsManagerFor(AudioType audioType, string managerName)
        {
            bool itIsThere = audioManagers.ContainsKey(audioType) && audioManagers[audioType] != null;
            if (!itIsThere)
            {
                audioManagers[audioType] = CreateAudioManager(managerName);
            }
        }

        protected virtual AudioManager CreateAudioManager(string name)
        {
            // We have separate game objects for the managers so we can check the
            // track-counts and such in the Scene view
            GameObject holdsManager = new GameObject(name);
            holdsManager.transform.SetParent(this.transform);
            AudioManager manager = new AudioManager(holdsManager);
            manager.Name = name;
            return manager;
        }

        protected IDictionary<AudioType, AudioManager> audioManagers = new Dictionary<AudioType, AudioManager>();

        protected AudioManager musicManager, sfxManager, voiceManager;

        protected virtual void OnEnable()
        {
            EnsureAudioManagersAreThere();
            ListenForAudioEvents();
        }

        protected virtual void ListenForAudioEvents()
        {
            AudioEvents.PlayMusic += musicManager.Play;
            AudioEvents.PlaySFX += sfxManager.Play;
            AudioEvents.PlayVoice += voiceManager.Play;

            AudioEvents.SetMusicVol += musicManager.SetVolume;
            AudioEvents.SetSFXVol += sfxManager.SetVolume;
            AudioEvents.SetVoiceVol += voiceManager.SetVolume;

            AudioEvents.SetMusicPitch += musicManager.SetPitch;
            AudioEvents.SetSFXPitch += sfxManager.SetPitch;
            AudioEvents.SetVoicePitch += voiceManager.SetPitch;

            AudioEvents.StopMusic += musicManager.Stop;
            AudioEvents.StopSFX += sfxManager.Stop;
            AudioEvents.StopVoice += voiceManager.Stop;
        }

        protected virtual void OnDisable()
        {
            UNListenForAudioEvents();
        }

        protected virtual void UNListenForAudioEvents()
        {
            AudioEvents.PlayMusic -= musicManager.Play;
            AudioEvents.PlaySFX -= sfxManager.Play;
            AudioEvents.PlayVoice -= voiceManager.Play;

            AudioEvents.SetMusicVol -= musicManager.SetVolume;
            AudioEvents.SetSFXVol -= sfxManager.SetVolume;
            AudioEvents.SetVoiceVol -= voiceManager.SetVolume;

            AudioEvents.SetMusicPitch -= musicManager.SetPitch;
            AudioEvents.SetSFXPitch -= sfxManager.SetPitch;
            AudioEvents.SetVoicePitch -= voiceManager.SetPitch;

            AudioEvents.StopMusic -= musicManager.Stop;
            AudioEvents.StopSFX -= sfxManager.Stop;
            AudioEvents.StopVoice -= voiceManager.Stop;
        }

        public float GetVolume(AudioArgs args)
        {
            var managerToUse = audioManagers[args.AudioType];
            return managerToUse.GetVolume(args);
        }

        public float GetPitch(AudioArgs args)
        {
            var managerToUse = audioManagers[args.AudioType];
            return managerToUse.GetPitch(args);
        }

    }
}