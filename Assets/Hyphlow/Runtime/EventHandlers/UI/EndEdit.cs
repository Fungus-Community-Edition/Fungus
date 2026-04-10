using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// The block will execute when the user finishes editing the text in the input field.
    /// </summary>
    [EventHandlerInfo("UI",
                      "End Edit",
                      "The block will execute when the user finishes editing the text in the input field.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class EndEdit : EventHandler
    {   
        [Tooltip("The UI Input Field that the user can enter text into")]
        [SerializeField] protected InputField targetInputField;
        
        protected virtual void Start()
        {
            targetInputField.onEndEdit.AddListener(OnEndEdit);
        }
        
        protected virtual void OnEndEdit(string text)
        {
            ExecuteBlock();
        }

        #region Public members

        public override string GetSummary()
        {
            if (targetInputField != null)
            {
                return targetInputField.name;
            }

            return "Error: no targetInputField set.";
        }

        #endregion
    }
}
