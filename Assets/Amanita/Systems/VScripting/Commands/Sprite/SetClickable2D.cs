using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Sets a Clickable2D component to be clickable / non-clickable.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Set Clickable 2D", 
                 "Sets a Clickable2D component to be clickable / non-clickable.")]
    [AddComponentMenu("")]
    public class SetClickable2D : Command
    {       
        [Tooltip("Reference to Clickable2D component on a gameobject")]
        [SerializeField] protected Clickable2D targetClickable2D;

        [Tooltip("Set to true to enable the component")]
        [SerializeField] protected BooleanData activeState;

        #region Public members

        public override void OnEnter()  
        {
            if (targetClickable2D != null)      
            {
                targetClickable2D.ClickEnabled = activeState.Value; 
            }
            
            Continue();
        }
        
        public override string GetSummary()
        {
            if (targetClickable2D == null)      
            {
                return "Error: No Clickable2D component selected";  
            }
            
            return targetClickable2D.gameObject.name;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(activeState.VarRef, variable) || base.HasReference(variable);
        }

        #endregion
    }
        
}
