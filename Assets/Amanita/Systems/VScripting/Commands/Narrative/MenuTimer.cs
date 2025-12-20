using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Amanita.VScripting;

namespace Amanita.DialogueSys.Commands
{
    /// <summary>
    /// Displays a timer bar and executes a target block if the player fails to select a menu option in time.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Menu Timer", 
                 "Displays a timer bar and executes a target block if the player fails to select a menu option in time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class MenuTimer : Command, IBlockCaller
    {
        [Tooltip("Length of time to display the timer for")]
        [SerializeField] protected FloatData _duration = new FloatData(1);

        [FormerlySerializedAs("targetSequence")]
        [Tooltip("Block to execute when the timer expires")]
        [SerializeField] protected Block targetBlock;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(_duration);
        }

        #region Public members

        public override void OnEnter()
        {
            var menuDialog = MenuDialog.GetMenuDialog();

            if (menuDialog != null &&
                targetBlock != null)
            {
                menuDialog.ShowTimer(_duration.Value, targetBlock);
            }

            Continue();
        }

        public override void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {
            if (targetBlock != null)
            {
                connectedBlocks.Add(targetBlock);
            }       
        }

        public override string GetSummary()
        {
            if (targetBlock == null)
            {
                return "Error: No target block selected";
            }

            return targetBlock.BlockName;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Narrative;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_duration.VarRef, variable) ||
                base.HasReference(variable);
        }

        public bool MayCallBlock(Block block)
        {
            return block == targetBlock;
        }
        
        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("duration")] public float durationOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (durationOLD != default(float))
            {
                _duration.Value = durationOLD;
                durationOLD = default(float);
            }
        }

        #endregion
    }
}
