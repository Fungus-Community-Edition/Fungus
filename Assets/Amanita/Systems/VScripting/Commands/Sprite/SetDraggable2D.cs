using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Sets a Draggable2D component to be draggable / non-draggable.
    /// </summary>
    [CommandInfo("Sprite",
                 "Set Draggable 2D",
                 "Sets a Draggable2D component to be draggable / non-draggable.")]
    [AddComponentMenu("")]
    public class SetDraggable2D : Command
    {      
        [Tooltip("Reference to Draggable2D component on a gameobject")]
        [SerializeField] protected Draggable2D targetDraggable2D;

        [Tooltip("Set to true to enable the component")]
        [SerializeField] protected BooleanData activeState;

        #region Public members

        public override void OnEnter() 
        {
            if (targetDraggable2D != null)         
            {
                targetDraggable2D.DragEnabled = activeState.Value;     
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetDraggable2D == null)         
            {
                return "Error: No Draggable2D component selected";     
            }

            return targetDraggable2D.gameObject.name;
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
