// Snippet added by ducksonthewater, 2019-01-03 - www.ducks-on-the-water.com

using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Changes the sprite on a SpriteRenderer.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Set Sprite", 
                 "Changes the sprite property of a list of Sprite Renderers.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetSprite : Command 
    {
        [Tooltip("List of sprites to set the sprite property on")]
        [SerializeField] protected List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

        [Tooltip("The sprite set on the target sprite renderers")]
        [SerializeField] protected Sprite sprite;

        #region Public members

        public override void OnEnter()
        {
            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                var spriteRenderer = spriteRenderers[i];
                spriteRenderer.sprite = sprite;
            }

            Continue();
        }
        
        public override string GetSummary()
        {
            string summary = "";
            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                var spriteRenderer = spriteRenderers[i];
                if (spriteRenderer == null)
                {
                    continue;
                }
                if (summary.Length > 0)
                {
                    summary += ", ";
                }
                summary += spriteRenderer.name;
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
            if (propertyName == "spriteRenderers")
            {
                return true;
            }

            return false;
        }

        public override void OnCommandAdded(Block parentBlock)
        {
            // Add a default empty entry
            spriteRenderers.Add(null);
        }

        #endregion
    }
}
