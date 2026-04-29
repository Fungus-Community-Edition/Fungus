using UnityEngine;
using System.Collections.Generic;
using AtMycelia.Amanita.DialogueSys;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita
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
        [Tooltip("Volume level of writing sound effects")]
        [Range(0, 1)]
        [FormerlySerializedAs("volume")]
        [SerializeField] protected float _volume = 1f;

        [Tooltip("Loop the audio when in Sound Effect mode. Has no effect in Beeps mode.")]
        [FormerlySerializedAs("loop")]
        [SerializeField] protected bool _loop = true;

        // If none is specifed then we use any AudioSource on the gameobject, and if that doesn't exist we create one.
        [Tooltip("AudioSource to use for playing sound effects. If none is selected then one will be created.")]
        [FormerlySerializedAs("targetAudioSource")]
        [SerializeField] protected AudioSource _targetAudioSource;

        [Tooltip("Type of sound effect to play when writing text")]
        [FormerlySerializedAs("audioMode")]
        [SerializeField] protected AudioMode _audioMode = AudioMode.Beeps;

        [Tooltip("List of beeps to randomly select when playing beep sound effects. Will play maximum of " +
            "one beep per character, with only one beep playing at a time.")]
        [FormerlySerializedAs("beepSounds")]
        [SerializeField] protected List<AudioClip> _beepSounds = new List<AudioClip>();

        [Tooltip("Long playing sound effect to play when writing text")]
        [FormerlySerializedAs("soundEffect")]
        [SerializeField] protected AudioClip _soundEffect;

        [Tooltip("Sound effect to play on user input (e.g. a click)")]
        [FormerlySerializedAs("inputSound")]
        [SerializeField] protected AudioClip _inputSound;

        protected float _targetVolume = 0f;

        // When true, a beep will be played on every written character glyph
        protected bool _playBeeps;

        // True when a voiceover clip is playing
        protected bool _playingVoiceover = false;

        protected AudioSource _lastUsedAudioSource;
        protected SayDialog _attachedSayDialog;

        // Time when current beep will have finished playing
        protected float _nextBeepTime;

        protected bool _useLegacyAudioLogic = true;

        protected virtual AudioSource VoiceOverAudioSource
        {
            get
            {
                return _targetAudioSource;
            }
        }

        protected virtual AudioSource EffectAudioSource
        {
            get
            {
                return _targetAudioSource;
            }
        }

        public float GetSecondsRemaining()
        {
            if (_playingVoiceover)
            {
                return _targetAudioSource.isPlaying ? _targetAudioSource.clip.length - _targetAudioSource.time : 0f;
            }
            else
            {
                return 0F;
            }
        }

        protected virtual void SetAudioMode(AudioMode mode)
        {
            _audioMode = mode;
        }

        protected virtual void Awake()
        {
            // Need to do this in Awake rather than Start due to init order issues
            _targetAudioSource = GetComponent<AudioSource>();
            _targetAudioSource.volume = 0f;
            _attachedSayDialog = GetComponent<SayDialog>();
        }

        protected virtual void Play(AudioClip audioClip)
        {
            if (EffectAudioSource == null ||
                (_audioMode == AudioMode.SoundEffect && _soundEffect == null && audioClip == null) ||
                (_audioMode == AudioMode.Beeps && _beepSounds.Count == 0))
            {
                return;
            }

            _lastUsedAudioSource = EffectAudioSource;

            _playingVoiceover = false;
            _lastUsedAudioSource.volume = 0f;
            _targetVolume = _volume;

            if (audioClip != null)
            {
                // Voice over clip provided
                _lastUsedAudioSource.clip = audioClip;
                _lastUsedAudioSource.loop = _loop;
                _lastUsedAudioSource.Play();
            }
            else if (_audioMode == AudioMode.SoundEffect &&
                     _soundEffect != null)
            {
                // Use sound effects defined in WriterAudio
                _lastUsedAudioSource.clip = _soundEffect;
                _lastUsedAudioSource.loop = _loop;
                _lastUsedAudioSource.Play();
            }
            else if (_audioMode == AudioMode.Beeps)
            {
                // Use beeps defined in WriterAudio
                _lastUsedAudioSource.clip = null;
                _lastUsedAudioSource.loop = false;
                _playBeeps = true;
            }
        }

        protected virtual void Pause()
        {
            if (_lastUsedAudioSource == null)
            {
                return;
            }

            // There's an audible click if you call audioSource.Pause() so instead just drop the volume to 0.
            _targetVolume = 0f;
        }

        protected virtual void Stop()
        {
            if (_lastUsedAudioSource == null)
            {
                return;
            }

            // There's an audible click if you call audioSource.Stop() so instead we just switch off
            // looping and let the audio stop automatically at the end of the clip
            _targetVolume = 0f;
            _lastUsedAudioSource.loop = false;
            _playBeeps = false;
            _playingVoiceover = false;

            //TODO force speaking character to stop
        }

        protected virtual void Resume()
        {
            if (_lastUsedAudioSource == null)
            {
                return;
            }

            _targetVolume = _volume;
        }

        protected virtual void Update()
        {
            if (_lastUsedAudioSource != null)
                _lastUsedAudioSource.volume = Mathf.MoveTowards(_lastUsedAudioSource.volume, _targetVolume, Time.deltaTime * 5f);
        }

        #region IWriterListener implementation

        public virtual void OnInput()
        {
            if (_inputSound != null)
            {
                // Assumes we're playing a 2D sound
                AudioSource.PlayClipAtPoint(_inputSound, Vector3.zero);
            }
        }

        public virtual void OnStartWritingNewText(AudioClip audioClip)
        {
            if (_playingVoiceover)
            {
                return;
            }
            Play(audioClip);
        }

        public virtual void OnPause()
        {
            if (_playingVoiceover)
            {
                return;
            }
            Pause();
        }

        public virtual void OnResume()
        {
            if (_playingVoiceover)
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
            if (_playingVoiceover)
            {
                return;
            }

            if (_playBeeps && _beepSounds.Count > 0)
            {
                _lastUsedAudioSource = EffectAudioSource;

                if (!_lastUsedAudioSource.isPlaying)
                {
                    if (_nextBeepTime < Time.realtimeSinceStartup)
                    {
                        _lastUsedAudioSource.clip = _beepSounds[Random.Range(0, _beepSounds.Count)];

                        if (_lastUsedAudioSource.clip != null)
                        {
                            _lastUsedAudioSource.loop = false;
                            _targetVolume = _volume;
                            _lastUsedAudioSource.Play();

                            float extend = _lastUsedAudioSource.clip.length;
                            _nextBeepTime = Time.realtimeSinceStartup + extend;
                        }
                    }
                }
            }
        }

        public virtual void OnVoiceover(AudioClip voiceoverClip)
        {
            if (VoiceOverAudioSource == null)
            {
                return;
            }

            _playingVoiceover = true;

            _lastUsedAudioSource = VoiceOverAudioSource;

            _lastUsedAudioSource.volume = _volume;
            _targetVolume = _volume;
            _lastUsedAudioSource.loop = false;
            _lastUsedAudioSource.clip = voiceoverClip;
            _lastUsedAudioSource.Play();
        }

        public void OnAllWordsWritten()
        {
        }

        #endregion
    }


}
