using UnityEngine;

namespace Amanita.VScripting.Legacy
{
    /// <summary>
    /// Plays looping game music. If any game music is already playing, it is stopped. Game music will continue playing across scene loads.
    /// </summary>
    [CommandInfo("Audio",
                 "Play Music",
                 "Plays looping game music. If any game music is already playing, it is stopped. Game music will continue playing across scene loads.")]
    [AddComponentMenu("")]
    public class PlayMusic : LegacyAudioCommand
    {
        [Tooltip("Music sound clip to play")]
        [SerializeField] protected AudioClip musicClip;

        [Tooltip("Time to begin playing in seconds. If the audio file is compressed, the time index may be inaccurate.")]
        [SerializeField] protected float atTime;

        [Tooltip("The music will start playing again at end.")]
        [SerializeField] protected bool loop = true;
    
        [Tooltip("Length of time to fade out previous playing music.")]
        [SerializeField] protected float fadeDuration = 1f;

        #region Public members

        public override void OnEnter()
        {
            float startTime = Mathf.Max(0, atTime);
            MusicManager.PlayMusic(musicClip, loop, fadeDuration, startTime);

            if (waitUntilFinished && !loop && musicClip != null)
            {
                _delayBeforeContinue = musicClip.length - startTime;
                Invoke(nameof(Continue), _delayBeforeContinue);
            }
            else
            {
                Continue();
            }
        }
                    
        public override string GetSummary()
        {
            if (musicClip == null)
            {
                return "Error: No music clip selected";
            }

            return musicClip.name;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        #endregion
    }
}