using AtMycelia.Hyphlow;
using System.Collections;
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
                 "Displays a timer bar and executes a target block if the player " +
                "fails to select a menu option in time.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [MovedFrom("AtMycelia.Amanita.DialogueSys.Commands")]
    public class MenuTimer : Command, IBlockCaller
    {
        [Tooltip("Length of time to display the timer for")]
        [SerializeField] protected FloatData _duration = new FloatData(1);

        [SerializeField]
        [Tooltip("Block to execute when the timer expires")]
        protected BlockReference _targetBlock = new BlockReference();

        [FormerlySerializedAs("targetSequence")]
        [FormerlySerializedAs("targetBlock")]
        [Tooltip("Block to execute when the timer expires")]
        [SerializeField] protected Block _oldTargetBlock;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_duration);
        }

        #region Public members

        public override void OnEnter()
        {
            StartCoroutine(ExecWait());
        }

        private IEnumerator ExecWait()
        {
            var menuDialog = MenuDialog.GetMenuDialog();
            var flowchart = ParentBlock.Owner as Flowchart;
            var targBlock = _targetBlock.Block;
            yield return menuDialog.ShowTimer(_duration.Value, Continue);
        }

        public override void GetConnectedBlocks(ref IList<IBlock> connectedBlocks)
        {
            if (_oldTargetBlock != null)
            {
                connectedBlocks.Add(_oldTargetBlock);
            }       
        }

        public override string GetSummary()
        {
            if (_oldTargetBlock == null)
            {
                return "Error: No target block selected";
            }

            return _oldTargetBlock.BlockName;
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
            bool result = ReferenceEquals(block, _oldTargetBlock);
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

            if (_oldTargetBlock != null)
            {
                _targetBlock ??= new BlockReference();
                _targetBlock.BlockOwner = _oldTargetBlock.Owner as IBlockSource;
                _targetBlock.Block = _oldTargetBlock;
                _oldTargetBlock = null;
            }
        }

        #endregion
    }
}
