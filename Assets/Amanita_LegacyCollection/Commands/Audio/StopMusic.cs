using UnityEngine;

namespace Amanita.VScripting.Legacy
{
    /// <summary>
    /// Stops the currently playing game music.
    /// </summary>
    [CommandInfo("Audio", 
                 "Stop Music", 
                 "Stops the currently playing game music.")]
    [AddComponentMenu("")]
    public class StopMusic : LegacyAudioCommand
    {
        #region Public members

        public override void OnEnter()
        {
            MusicManager.StopMusic();
            Continue();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        #endregion
    }
}