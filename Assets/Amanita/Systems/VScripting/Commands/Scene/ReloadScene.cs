using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Reload the current scene
    /// </summary>
    [CommandInfo("Scene",
                 "Reload",
                 "Reload the current scene")]
    [AddComponentMenu("")]
    public class ReloadScene : Command
    {
        [Tooltip("Image to display while loading the scene")]
        [SerializeField]
        protected Texture2D loadingImage;

        public override void OnEnter()
        {
            SceneLoader.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, loadingImage);

            Continue();
        }

        public override string GetSummary()
        {
            return "";
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }
    }
}
