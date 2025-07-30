


using UnityEngine;

namespace Amanita
{
    /// <summary>
    /// Stops the currently playing game music.
    /// </summary>
    [CommandInfo("Audio", 
                 "Stop Music", 
                 "Stops the currently playing game music.")]
    [AddComponentMenu("")]
    public class StopMusic : Command
    {
        #region Public members

        public override void OnEnter()
        {
            var musicManager = AmanitaManager.S.MusicManager;

            musicManager.StopMusic();

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}