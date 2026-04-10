using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Quits the application. Does not work in Editor or Webplayer builds. Shouldn't generally be used on iOS.
    /// </summary>
    [CommandInfo("Flow", 
                 "Quit", 
                 "Quits the application. Does not work in Editor or Webplayer builds. Shouldn't generally be used on iOS.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Quit : Command 
    {
        #region Public members

        public override void OnEnter()
        {
            Application.Quit();

            // On platforms that don't support Quit we just continue onto the next command
            Continue();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        #endregion
    }
}
