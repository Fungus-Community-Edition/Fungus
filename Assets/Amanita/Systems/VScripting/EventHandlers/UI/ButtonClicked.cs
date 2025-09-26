using UnityEngine;
using UnityEngine.UI;

namespace Amanita.VScripting.EventHandlers
{
    /// <summary>
    /// The block will execute when the user clicks on the target UI button object.
    /// </summary>
    [EventHandlerInfo("UI",
                      "Button Clicked",
                      "The block will execute when the user clicks on the target UI button object.")]
    [AddComponentMenu("")]
    public class ButtonClicked : EventHandler
    {   
        [Tooltip("The UI Button that the user can click on")]
        [SerializeField] protected Button targetButton;

        #region Public members

        public virtual void Start()
        {
            if (targetButton != null)
            {
                targetButton.onClick.AddListener(OnButtonClick);
            }
        }
        
        protected virtual void OnButtonClick()
        {
            ExecuteBlock();
        }

        public override string GetSummary()
        {
            if (targetButton != null)
            {
                return targetButton.name;
            }

            return "Error: no targetButton set.";
        }

        #endregion
    }
}
