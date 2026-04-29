using AtMycelia.Amanita;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Reload the current scene
    /// </summary>
    [CommandInfo("Scene",
                 "Reload",
                 "Reload the current scene")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ReloadScene : Command
    {
        [Tooltip("Image to display while loading the scene")]
        [SerializeField]
        protected TextureData _loadingImage = new TextureData();

        public override void OnEnter()
        {
            if (_loadingImage.Value != null && _loadingImage.Value is not Texture2D)
            {
                string warningMessage = $"ReloadScene Command on Flowchart {this.name}, Block {ParentBlock.BlockName} " +
                    $"at index {CommandIndex} has a loading image that is not a Texture2D. The image will be ignored.";
                Debug.LogWarning(warningMessage, this);
            }

            var activeScene = SceneManager.GetActiveScene();
            var sceneName = activeScene.name;
            SceneLoader.LoadScene(sceneName, _loadingImage.Value as Texture2D);

            Continue();
        }

        public override string GetSummary()
        {
            string result = "";
            if (_loadingImage.Value != null)
            {
                result += $"Loading Image: {_loadingImage.Value.name}";
            }
            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldLoadingImage != null)
            {
                _loadingImage.Value = _oldLoadingImage;
                _oldLoadingImage = null;
            }
        }

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("loadingImage")]
        protected Texture2D _oldLoadingImage;
    }
}
