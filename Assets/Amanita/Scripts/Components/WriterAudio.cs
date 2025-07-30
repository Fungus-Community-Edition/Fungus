using UnityEngine;
using System.Collections.Generic;
using Amanita.Myceliaudio;
using Amanita.DialogueSys;

namespace Amanita
{
    /// <summary>
    /// Type of audio effect to play.
    /// </summary>
    public enum AudioMode
    {
        /// <summary> Use short beep sound effects. </summary>
        Beeps,
        /// <summary> Use long looping sound effect. </summary>
        SoundEffect,
    }

    /// <summary>
    /// Manages audio effects for Dialogs.
    /// </summary>
    public class WriterAudio : MonoBehaviour, IWriterListener
    {

        [SerializeField] protected int voiceOverAudioTrack = 128;
        [SerializeField] protected int effectAudioTrack = 129;
        [SerializeField] protected int beepAudioTrack = 130;
        [SerializeField] protected int inputAudioTrack = 131;
        // ^We want beeps to be on a different track than the other sound effects so we can have both
        // playing at the same time.

        [Tooltip("Loop the audio when in Sound Effect mode. Has no effect in Beeps mode.")]
        [SerializeField] protected bool loop = true;

        [Tooltip("Type of sound effect to play when writing text")]
        [SerializeField] protected AudioMode audioMode = AudioMode.Beeps;

        [Tooltip("List of beeps to randomly select when playing beep sound effects. Will play maximum of one beep per character, with only one beep playing at a time.")]
        [SerializeField] protected List<AudioClip> beepSounds = new List<AudioClip>();

        [Tooltip("Long playing sound effect to play when writing text")]
        [SerializeField] protected AudioClip soundEffect;

        [Tooltip("Sound effect to play on user input (e.g. a click)")]
        [SerializeField] protected AudioClip inputSound;

        // When true, a beep will be played on every written character glyph
        protected bool playBeeps;

        // True when a voiceover clip is playing
        protected bool playingVoiceover = false;

        protected AudioSource lastUsedAudioSource;
        protected SayDialog attachedSayDialog;

        // Time when current beep will have finished playing
        protected float whenBeepDonePlaying;

        [Tooltip("If true, legacy voiceover logic used and any audio clips will be played through the targetAudioSource," +
            " same one that sfx and beeps are played through.")]
        [SerializeField] protected bool useLegacyAudioLogic = false;

        public float GetSecondsRemaining()
        {
            int eitherDoneOrNotPlaying = 0;
            float result = eitherDoneOrNotPlaying;

            if (playingVoiceover)
            {
                //bool playingVoiceClipRightNow = AudioSystem.S.GetIsPlaying(TrackGroup.Voice, voiceOverAudioTrack);
                // ^For some reason, this can be true even when the clip is done playing. Hence why instead of 
                // checking whether the clip is playing, we check if the clip is null.

                AudioClip voiceClip = AudioSystem.S.GetClipPlayingAt(TrackGroup.Voice, voiceOverAudioTrack);
                float howFarAlong = AudioSystem.S.GetMainTime(TrackGroup.Voice, voiceOverAudioTrack);

                if (voiceClip != null)
                {
                    result = voiceClip.length - howFarAlong;
                }
            }

            return result;
        }

        protected virtual void SetAudioMode(AudioMode mode)
        {
            audioMode = mode;
        }

        protected virtual void Awake()
        {
            PrepAudioArgs();
            attachedSayDialog = GetComponent<SayDialog>();
        }

        protected virtual void PrepAudioArgs()
        {
            playVoiceOver = new()
            {
                Track = voiceOverAudioTrack,
                TrackGroup = TrackGroup.Voice,
                MainClip = null, // We expect this to be set in Play()
                Loop = false,
            };

            playBeepSfx = new()
            {
                Track = beepAudioTrack,
                TrackGroup = TrackGroup.SoundFX,
                MainClip = GetRandomBeep(),
                Loop = false,
            };

            playInputSfx = new()
            {
                Track = inputAudioTrack,
                TrackGroup = TrackGroup.SoundFX,
                MainClip = inputSound,
                Loop = false,
            };

            playOtherSfx = new()
            {
                Track = effectAudioTrack,
                TrackGroup = TrackGroup.SoundFX,
                MainClip = soundEffect,
                Loop = loop,
            };
        }

        protected PlayAudioArgs playVoiceOver, playBeepSfx, playInputSfx, playOtherSfx;

        protected virtual AudioClip GetRandomBeep()
        {
            if (beepSounds.Count == 0)
            {
                return null;
            }
            int index = Random.Range(0, beepSounds.Count);
            return beepSounds[index];
        }

        protected virtual void Play(AudioClip voiceOverClip)
        {
            bool weHaveSfxOrVoiceClipToPlay = voiceOverClip != null || soundEffect != null;
            bool weHaveBeepsToPlay = beepSounds.Count > 0;
            if ((audioMode == AudioMode.SoundEffect && weHaveSfxOrVoiceClipToPlay) ||
                (audioMode == AudioMode.Beeps && !weHaveBeepsToPlay))
            {
                return;
            }

            playingVoiceover = false;

            if (voiceOverClip != null)
            {
                // Voice over clip provided
                playVoiceOver.Loop = loop;
                AudioSystem.S.SetTrackVol(TrackGroup.Voice, playVoiceOver.Track, normalAudibility);
                AudioSystem.S.Play(playVoiceOver);
            }
            else if (audioMode == AudioMode.SoundEffect &&
                     soundEffect != null)
            {
                // Use sound effects defined in WriterAudio
                playOtherSfx.Loop = loop;
                AudioSystem.S.Play(playOtherSfx);
            }
            else if (audioMode == AudioMode.Beeps)
            {
                // Use beeps defined in WriterAudio
                playBeeps = true;
            }
        }

        protected virtual void Pause()
        {
            if (lastUsedAudioSource == null)
            {
                return;
            }

            // To avoid an audible click we'd otherwise get if we called audioSource.Stop()
            SetTrackVolsTo(silent);
        }

        protected static int silent = 0;

        protected virtual void SetTrackVolsTo(float newVol)
        {
            AudioSystem.S.SetTrackVol(TrackGroup.Voice, voiceOverAudioTrack, newVol);
            AudioSystem.S.SetTrackVol(TrackGroup.SoundFX, effectAudioTrack, newVol);
            AudioSystem.S.SetTrackVol(TrackGroup.SoundFX, beepAudioTrack, newVol);
            AudioSystem.S.SetTrackVol(TrackGroup.SoundFX, inputAudioTrack, newVol);
        }

        protected virtual void Stop()
        {
            if (lastUsedAudioSource == null)
            {
                return;
            }

            SetTrackVolsTo(silent);
            SetTrackLooping(false);

            playBeeps = false;
            playingVoiceover = false;
        }

        protected virtual void SetTrackLooping(bool loop)
        {
            // No need for this. If we want something to play with or without looping,
            // we can easily just let the AudioSystem know
            //AudioSystem.S.SetLoop(TrackGroup.Voice, voiceOverAudioTrack, loop);
            //AudioSystem.S.SetLoop(TrackGroup.SoundFX, effectAudioTrack, loop);
            //AudioSystem.S.SetLoop(TrackGroup.SoundFX, beepAudioTrack, loop);
            //AudioSystem.S.SetLoop(TrackGroup.SoundFX, inputAudioTrack, loop);
        }

        protected virtual void Resume()
        {
            if (lastUsedAudioSource == null)
            {
                return;
            }

            SetTrackVolsTo(normalAudibility);
        }

        protected static float normalAudibility = 100f;
        // ^Remember, the actual volume a track is playing at is anchored by the group it is
        // assigned to. Thus, setting this to 100f means that the track will play at whatever
        // volume the group is set to. This is the default value for all tracks, so it should
        // be safe to use.

        protected virtual void Update()
        {
            //if (lastUsedAudioSource != null)
            //    lastUsedAudioSource.volume = Mathf.MoveTowards(lastUsedAudioSource.volume, targetVolume, Time.deltaTime * 5f);
            // ^Seems that in the orig, we tried going for a fade effect. Best cut this out for now and later decide 
            // at what point we should start doing the fading (since doing it every frame like in the orig is a bit overkill)
        }

        #region IWriterListener implementation

        public virtual void OnInput()
        {
            if (playInputSfx.MainClip != null)
            {
                // Assumes we're playing a 2D sound, which Myceliaudio does by default
                AudioSystem.S.Play(playInputSfx);
            }
        }

        public virtual void OnStartWritingNewText(AudioClip audioClip)
        {
            if (playingVoiceover)
            {
                return;
            }
            Play(audioClip);
        }

        public virtual void OnPause()
        {
            if (playingVoiceover) // Since at the time of this writing, we don't intend to support pausing voiceovers
            {
                return;
            }
            Pause();
        }

        public virtual void OnResume()
        {
            if (playingVoiceover)
            {
                return;
            }
            Resume();
        }

        public virtual void OnEnd(bool stopAudio)
        {
            if (stopAudio)
            {
                Stop();
            }
        }

        public virtual void OnGlyphWritten()
        {
            if (playingVoiceover || AudioSystem.S == null)
            {
                return;
                // If AudioSystem.S is null, chances are that it's because the application is shutting down
            }

            if (playBeeps && beepSounds.Count > 0)
            {
                bool playingBeepsRightNow = AudioSystem.S.GetIsPlaying(TrackGroup.SoundFX, beepAudioTrack);
                if (!playingBeepsRightNow)
                {
                    bool lastBeepDonePlaying = whenBeepDonePlaying < Time.realtimeSinceStartup;
                    if (lastBeepDonePlaying)
                    {
                        AudioClip beepToUse = GetRandomBeep();
                        playBeepSfx.MainClip = beepToUse;
                        playBeepSfx.Loop = false;
                        AudioSystem.S.Play(playBeepSfx);
                        //
                        float extend = (float)beepToUse.PreciseLength();
                        whenBeepDonePlaying = Time.realtimeSinceStartup + extend;
                        
                    }
                }
            }
        }

        public virtual void OnVoiceover(AudioClip voiceoverClip)
        {
            playingVoiceover = true;

            playVoiceOver.Loop = false;
            playVoiceOver.MainClip = voiceoverClip;
            AudioSystem.S.Play(playVoiceOver);
        }

        public void OnAllWordsWritten()
        {
        }

        #endregion
    }
}
