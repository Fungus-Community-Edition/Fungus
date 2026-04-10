using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Marks the end of a conditional block.
    /// </summary>
    [CommandInfo("Flow", 
                 "End", 
                 "Marks the end of a conditional block.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class End : Command
    {
        #region Public members

        /// <summary>
        /// Set to true by looping constructs to allow for loops to occur
        /// </summary>
        public virtual bool Loop { get; set; }

        /// <summary>
        /// Set to the index of the owning looping construct
        /// </summary>
        public virtual int LoopBackIndex { get; set; }

        public override void OnEnter()
        {
            if (Loop)
            {
                Continue(LoopBackIndex);
                return;
            }

            Continue();
        }

        public override bool CloseBlock()
        {
            return true;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.ConditionalLogic;
        }

        #endregion
    }
}
