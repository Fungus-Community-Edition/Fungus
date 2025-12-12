using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Stops the currently playing game music.
    /// </summary>
    [CommandInfo("Audio", 
                 "Stop Ambiance", 
                 "Stops the currently playing game ambiance.")]
    [AddComponentMenu("")]
    public class StopAmbiance : Command
    {
        #region Public members

        public override void OnEnter()
        {
            var musicManager = MusicManager.S;

            musicManager.StopAmbiance();

            Continue();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        #endregion
    }
}