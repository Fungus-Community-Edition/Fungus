using System.Collections.Generic;

namespace CGT.FungusExt.Myceliaudio
{
    public static class AudioEvents
    {
        public static event AudioHandler PlayMusic = delegate { },
            PlaySFX = delegate { },
            PlayVoice = delegate { },

            SetMusicVol = delegate { },
            SetSFXVol = delegate { },
            SetVoiceVol = delegate { },

            SetMusicPitch = delegate { },
            SetSFXPitch = delegate { },
            SetVoicePitch = delegate { },

            StopMusic = delegate { },
            StopSFX = delegate { },
            StopVoice = delegate { };

        static AudioEvents()
        {
            PreparePlayEvents();
            PrepareVolEvents();
            PreparePitchEvents();
            PrepareStopEvents();
        }

        private static void PreparePlayEvents()
        {
            PlayEvents.Add(Music, TriggerPlayMusic);
            PlayEvents.Add(SFX, TriggerPlaySFX);
            PlayEvents.Add(Voice, TriggerPlayVoice);
        }

        private static Dictionary<AudioType, AudioHandler> PlayEvents =
            new Dictionary<AudioType, AudioHandler>();

        private static AudioType Music { get { return AudioType.Music; } }
        private static AudioType SFX { get { return AudioType.SFX; } }
        private static AudioType Voice { get { return AudioType.Voice; } }

        private static void TriggerPlayMusic(AudioArgs args)
        {
            PlayMusic.Invoke(args);
        }

        private static void TriggerPlaySFX(AudioArgs args)
        {
            PlaySFX.Invoke(args);
        }

        private static void TriggerPlayVoice(AudioArgs args)
        {
            PlayVoice.Invoke(args);
        }

        private static void PrepareVolEvents()
        {
            SetVolEvents.Add(Music, TriggerSetMusicVolume);
            SetVolEvents.Add(SFX, TriggerSetSFXVolume);
            SetVolEvents.Add(Voice, TriggerSetVoiceVolume);
        }

        private static Dictionary<AudioType, AudioHandler> SetVolEvents =
            new Dictionary<AudioType, AudioHandler>();

        private static void TriggerSetMusicVolume(AudioArgs args)
        {
            SetMusicVol.Invoke(args);
        }

        private static void TriggerSetSFXVolume(AudioArgs args)
        {
            SetSFXVol.Invoke(args);
        }

        private static void TriggerSetVoiceVolume(AudioArgs args)
        {
            SetVoiceVol.Invoke(args);
        }

        private static void PreparePitchEvents()
        {
            PitchEvents[Music] = TriggerSetMusicPitch;
            PitchEvents[SFX] = TriggerSetSFXPitch;
            PitchEvents[Voice] = TriggerSetVoicePitch;
        }

        private static Dictionary<AudioType, AudioHandler> PitchEvents =
            new Dictionary<AudioType, AudioHandler>();

        private static void TriggerSetMusicPitch(AudioArgs args)
        {
            SetMusicPitch.Invoke(args);
        }

        private static void TriggerSetSFXPitch(AudioArgs args)
        {
            SetSFXPitch.Invoke(args);
        }

        private static void TriggerSetVoicePitch(AudioArgs args)
        {
            SetVoicePitch.Invoke(args);
        }

        private static void PrepareStopEvents()
        {
            StopEvents.Add(Music, TriggerStopMusic);
            StopEvents.Add(SFX, TriggerStopSFX);
            StopEvents.Add(Voice, TriggerStopVoice);
        }

        private static Dictionary<AudioType, AudioHandler> StopEvents = 
            new Dictionary<AudioType, AudioHandler>();

        private static void TriggerStopMusic(AudioArgs args)
        {
            StopMusic.Invoke(args);
        }

        private static void TriggerStopSFX(AudioArgs args)
        {
            StopSFX.Invoke(args);
        }

        private static void TriggerStopVoice(AudioArgs args)
        {
            StopVoice.Invoke(args);
        }


        public static void TriggerPlayAudio(AudioArgs args)
        {
            PlayEvents[args.AudioType].Invoke(args);
        }

        public static void TriggerSetVolume(AudioArgs args)
        {
            SetVolEvents[args.AudioType].Invoke(args);
        }

        public static void TriggerStopAudio(AudioArgs args)
        {
            StopEvents[args.AudioType].Invoke(args);
        }
        
        public static void TriggerSetPitch(AudioArgs args)
        {
            PitchEvents[args.AudioType].Invoke(args);
        }

    }
}