using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Changes the Image property on a UI element.
    /// </summary>
    [CommandInfo("UI", 
                 "Set UI Image", 
                 "Changes the Image property of a list of UI Images.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.Commands")]
    public class SetUIImage : Command 
    {
        [Tooltip("List of UI Images to set the source image property on")]
        [SerializeField] protected List<Image> images = new List<Image>();

        [Tooltip("The sprite set on the source image property")]
        [SerializeField] protected Sprite sprite;

        #region Public members

        public override void OnEnter()
        {
            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                image.sprite = sprite;
            }

            Continue();
        }
        
        public override string GetSummary()
        {
            string summary = "";
            for (int i = 0; i < images.Count; i++)
            {
                var targetImage = images[i];
                if (targetImage == null)
                {
                    continue;
                }
                if (summary.Length > 0)
                {
                    summary += ", ";
                }
                summary += targetImage.name;
            }

            if (summary.Length == 0)
            {
                return "Error: No sprite selected";
            }

            return summary + " = " + sprite;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool IsReorderableArray(string propertyName)
        {
            if (propertyName == "images")
            {
                return true;
            }

            return false;
        }

        public override void OnCommandAdded(Block parentBlock)
        {
            // Add a default empty entry
            images.Add(null);
        }

        #endregion
    }
}
