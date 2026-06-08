using AtMycelia.Hyphlow;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita.DialogueSys.VScripting
{
    /// <summary>
    /// Displays a timer bar and executes a target block if the player fails to select a menu option in time.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Menu Timer", 
                 "Displays a timer bar and executes a target block if the player fails to select a menu option in time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [MovedFrom("AtMycelia.Amanita.DialogueSys.Commands")]
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
            _variableDataCache.Add(_duration);
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

        public override void GetConnectedBlocks(ref IList<IBlock> connectedBlocks)
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

        public override bool HasReference(IVariable variable)
        {
            return ReferenceEquals(_duration.VarRef, variable) ||
                base.HasReference(variable);
        }

        public bool MayCallBlock(IBlock block)
        {
            bool result = ReferenceEquals(block, targetBlock);
            return result;
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
