using UnityEngine;

namespace Amanita
{
    /// <summary>
    /// Music manager which provides basic music and sound effect functionality.
    /// Music playback persists across scene loads.
    /// </summary>
    //[RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        protected AudioSource audioSourceMusic;
        protected AudioSource audioSourceAmbiance;
        protected AudioSource audioSourceSoundEffect;
        protected AudioSource audioSourceDefaultVoice;
        protected AudioSource audioSourceWriterSoundEffect;

        const int RequiredAudioSources = 5;

        public AudioSource DefaultVoiceAudioSource { get { return audioSourceDefaultVoice; } }
        public AudioSource WriterSoundEffectAudioSource { get { return audioSourceWriterSoundEffect; } }

        void Reset()
        {
            int audioSourceCount = this.GetComponents<AudioSource>().Length;
            for (int i = 0; i < RequiredAudioSources - audioSourceCount; i++)
                gameObject.AddComponent<AudioSource>();
        }

        public virtual void Init()
        {
            Reset();
            AudioSource[] audioSources = GetComponents<AudioSource>();
            audioSourceMusic = audioSources[0];
            audioSourceAmbiance = audioSources[1];
            audioSourceSoundEffect = audioSources[2];
            audioSourceDefaultVoice = audioSources[3];
            audioSourceWriterSoundEffect = audioSources[4];

            audioSourceMusic.outputAudioMixerGroup = AmanitaManager.S.MainAudioMixer.MusicGroup;
            audioSourceSoundEffect.outputAudioMixerGroup = AmanitaManager.S.MainAudioMixer.SFXGroup;
            audioSourceAmbiance.outputAudioMixerGroup = audioSourceSoundEffect.outputAudioMixerGroup;
            audioSourceDefaultVoice.outputAudioMixerGroup = AmanitaManager.S.MainAudioMixer.VoiceGroup;
            audioSourceWriterSoundEffect.outputAudioMixerGroup = audioSourceSoundEffect.outputAudioMixerGroup;

            fadeMusicVolume.Target = fadeMusicPitch.Target = audioSourceMusic;
            fadeAmbianceVolume.Target = fadeAmbiancePitch.Target = audioSourceAmbiance;
        }

        protected AudioTweenArgs fadeMusicVolume = new AudioTweenArgs(), fadeMusicPitch = new AudioTweenArgs(),
            fadeAmbianceVolume = new AudioTweenArgs(), fadeAmbiancePitch = new AudioTweenArgs();

        protected virtual void Start()
        {
            audioSourceMusic.playOnAwake = false;
            audioSourceMusic.loop = true;
        }

        #region Public members

        /// <summary>
        /// Plays game music using an audio clip.
        /// One music clip may be played at a time.
        /// </summary>
        public void PlayMusic(AudioClip musicClip, bool loop, float fadeDuration, float atTime)
        {
            if (audioSourceMusic == null || audioSourceMusic.clip == musicClip)
            {
                return;
            }

            if (Mathf.Approximately(fadeDuration, 0f))
            {
                audioSourceMusic.clip = musicClip;
                audioSourceMusic.loop = loop;
                audioSourceMusic.time = atTime;  // May be inaccurate if the audio source is compressed http://docs.unity3d.com/ScriptReference/AudioSource-time.html BK
                audioSourceMusic.Play();
            }
            else
            {
                float startVolume = audioSourceMusic.volume;

                fadeMusicVolume.BaseValue = startVolume;
                fadeMusicVolume.TargetValue = 0;
                fadeMusicVolume.HowLongToTake = fadeDuration;
                fadeMusicVolume.OnComplete = (AudioTweenArgs args) =>
                {
                    // Play new music
                    audioSourceMusic.volume = args.BaseValue;
                    audioSourceMusic.clip = musicClip;
                    audioSourceMusic.loop = loop;
                    audioSourceMusic.time = atTime;  // May be inaccurate if the audio source is compressed http://docs.unity3d.com/ScriptReference/AudioSource-time.html BK
                    audioSourceMusic.Play();
                };

                TweenManager.TweenAudioSourceVolume(fadeMusicVolume);
            }
        }

        /// <summary>
        /// Plays a sound effect once, at the specified volume.
        /// </summary>
        /// <param name="soundClip">The sound effect clip to play.</param>
        /// <param name="volume">The volume level of the sound effect.</param>
        public virtual void PlaySound(AudioClip soundClip, float volume)
        {
            audioSourceSoundEffect.PlayOneShot(soundClip, volume);
        }

        /// <summary>
        /// Plays a sound effect with optional looping values, at the specified volume.
        /// </summary>
        /// <param name="soundClip">The sound effect clip to play.</param>
        /// <param name="loop">If the audioclip should loop or not.</param>
        /// <param name="volume">The volume level of the sound effect.</param>
        public virtual void PlayAmbianceSound(AudioClip soundClip, bool loop, float volume)
        {
            audioSourceAmbiance.loop = loop;
            audioSourceAmbiance.clip = soundClip;
            audioSourceAmbiance.volume = volume;
            audioSourceAmbiance.Play();
        }

        /// <summary>
        /// Shifts the game music pitch to required value over a period of time.
        /// </summary>
        /// <param name="pitch">The new music pitch value. Between 0 and 200.</param>
        /// <param name="duration">The length of time in seconds needed to complete the pitch change.</param>
        /// <param name="onComplete">A delegate method to call when the pitch shift has completed.</param>
        public virtual void SetAudioPitch(float pitch, float duration, System.Action onComplete = null)
        {
            // We don't want any tweens to get in the way of setting the pitch 
            // (be it immediately or through another tween), so...

            onComplete += delegate { };
            if (Mathf.Approximately(duration, 0f))
            {
                audioSourceMusic.pitch = pitch / 100f;
                audioSourceAmbiance.pitch = pitch / 100f;
                onComplete();
                return;
            }

            fadeMusicPitch.BaseValue = fadeAmbiancePitch.BaseValue = audioSourceMusic.pitch;
            fadeMusicPitch.TargetValue = fadeAmbiancePitch.TargetValue = pitch;
            fadeMusicPitch.HowLongToTake = fadeAmbiancePitch.HowLongToTake = duration;

            fadeMusicPitch.OnComplete = (AudioTweenArgs args) => onComplete();
            // ^ Best assign this to just one of the args; we don't want onComplete to execute twice
            // through just one call of this func

            TweenManager.TweenAudioSourcePitch(fadeMusicPitch);
            TweenManager.TweenAudioSourcePitch(fadeAmbiancePitch);
        }

        /// <summary>
        /// Fades the game music volume to required level over a period of time.
        /// </summary>
        /// <param name="volume">The new music volume value (range from 0 for silent to 100 for max)</param>
        /// <param name="duration">The length of time in seconds needed to complete the volume change.</param>
        /// <param name="onComplete">Delegate function to call when fade completes.</param>
        public virtual void SetAudioVolume(float volume, float duration, System.Action onComplete)
        {
            onComplete += delegate { };
            if (Mathf.Approximately(duration, 0f))
            {
                audioSourceMusic.volume = volume;
                audioSourceAmbiance.volume = volume;
                onComplete();
                return;
            }

            fadeMusicVolume.BaseValue = fadeAmbianceVolume.BaseValue = audioSourceMusic.volume;
            fadeMusicVolume.TargetValue = fadeAmbianceVolume.TargetValue = volume;
            fadeMusicVolume.HowLongToTake = fadeAmbianceVolume.HowLongToTake = duration;

            fadeMusicVolume.OnComplete = (AudioTweenArgs args) => onComplete();
            // ^ Best assign this to just one of the args; we don't want onComplete to execute twice
            // through just one call of this func

            TweenManager.TweenAudioSourceVolume(fadeMusicVolume);
            TweenManager.TweenAudioSourceVolume(fadeAmbianceVolume);
        }

        /// <summary>
        /// Stops playing game music.
        /// </summary>
        public virtual void StopMusic()
        {
            audioSourceMusic.Stop();
            audioSourceMusic.clip = null;
        }

        /// <summary>
        /// Stops playing game ambiance.
        /// </summary>
        public virtual void StopAmbiance()
        {
            audioSourceAmbiance.Stop();
            audioSourceAmbiance.clip = null;
        }

        #endregion
    }
}