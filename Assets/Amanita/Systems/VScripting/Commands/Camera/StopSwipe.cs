using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Deactivates swipe panning mode.
    /// </summary>
    [CommandInfo("Camera", 
                 "Stop Swipe", 
                 "Deactivates swipe panning mode.")]
    [AddComponentMenu("")]
    public class StopSwipe : Command 
    {
        #region Public members

        public override void OnEnter()
        {
            var cameraManager = AmanitaManager.S.CameraManager;

            cameraManager.StopSwipePan();

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255);
        }

        #endregion
    }
}
