using UnityEngine;

namespace Amanita.VScripting.Legacy
{
    /// <summary>
    /// Plays a once-off sound effect. Multiple sound effects can be played at the same time.
    /// </summary>
    [CommandInfo("Audio",
                 "Play Ambiance Sound",
                 "Plays a background sound to be overlayed on top of the music. Only one Ambiance can be played at a time.")]
    [AddComponentMenu("")]
    public class PlayAmbianceSound : LegacyAudioCommand
    {
        [Tooltip("Sound effect clip to play")]
        [SerializeField]
        protected AudioClip soundClip;

        [Range(0, 1)]
        [Tooltip("Volume level of the sound effect")]
        [SerializeField]
        protected float volume = 1;
        
        [Tooltip("Sound effect clip to play")]
        [SerializeField]
        protected bool loop;

        #region Public members

        public override void OnEnter()
        {
            if (soundClip == null)
            {
                Continue();
                return;
            }

            MusicManager.PlayAmbianceSound(soundClip, loop, volume);

            if (waitUntilFinished)
            {
                Invoke(nameof(Continue), soundClip.length);
            }
            else
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (soundClip == null)
            {
                return "Error: No sound clip selected";
            }

            return soundClip.name;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        #endregion
    }
}
