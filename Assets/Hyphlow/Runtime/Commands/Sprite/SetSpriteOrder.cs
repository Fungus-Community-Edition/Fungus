using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Controls the render order of sprites by setting the Order In Layer property of a list of sprites.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Set Sprite Order", 
                 "Controls the render order of sprites by setting the Order In Layer property of a list of sprites.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetSpriteOrder : Command 
    {
        [Tooltip("List of sprites to set the order in layer property on")]
        [SerializeField] protected List<SpriteRenderer> targetSprites = new List<SpriteRenderer>();

        [Tooltip("The order in layer value to set on the target sprites")]
        [SerializeField] protected IntegerData orderInLayer;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(orderInLayer);
        }

        #region Public members

        public override void OnEnter()
        {
            for (int i = 0; i < targetSprites.Count; i++)
            {
                var spriteRenderer = targetSprites[i];
                spriteRenderer.sortingOrder = orderInLayer;
            }

            Continue();
        }
        
        public override string GetSummary()
        {
            string summary = "";
            for (int i = 0; i < targetSprites.Count; i++)
            {
                var spriteRenderer = targetSprites[i];
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
                return "Error: No cursor sprite selected";
            }

            return summary + " = " + orderInLayer.Value;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool IsReorderableArray(string propertyName)
        {
            if (propertyName == "targetSprites")
            {
                return true;
            }

            return false;
        }

        public override void OnCommandAdded(Block parentBlock)
        {
            // Add a default empty entry
            targetSprites.Add(null);
        }

        public override bool HasReference(Variable variable)
        {
            return orderInLayer.integerRef == variable || base.HasReference(variable);
        }

        #endregion
    }
}
