using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Supported modes for calling a block.
    /// </summary>
    public enum CallMode
    {
        /// <summary> Stop executing the current block after calling. </summary>
        Stop,
        /// <summary> Continue executing the current block after calling  </summary>
        Continue,
        /// <summary> Wait until the called block finishes executing, then continue executing current block. </summary>
        WaitUntilFinished,
        /// <summary> Stop executing the current block before attempting to call. This allows for circular calls within the same frame </summary>
        StopThenCall,

        /// <summary>
        /// Mainly for debug. In production, functions the same as Stop.
        /// </summary>
        Null,
    }

    /// <summary>
    /// Execute another block in the same Flowchart as the command, or in a different Flowchart.
    /// </summary>
    [CommandInfo("Flow", 
                 "Call", 
                 "Execute another block in the same Flowchart as the command, or in a different Flowchart.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Call : Command, IBlockCaller
    {
        [Tooltip("Flowchart which contains the block to execute. If none is specified then the current Flowchart is used.")]
        [FormerlySerializedAs("targetFlowchart")]
        [SerializeField] protected Flowchart _targetFlowchart;

        [FormerlySerializedAs("targetSequence")]
        [Tooltip("Block to start executing")]
        [FormerlySerializedAs("targetBlock")]
        [SerializeField] protected Block _targetBlock;

        [Tooltip("Label to start execution at. Takes priority over startIndex.")]
        [FormerlySerializedAs("startLabel")]
        [SerializeField] protected StringData _startLabel = new StringData();

        [Tooltip("Command index to start executing")]
        [FormerlySerializedAs("startIndex")]
        [SerializeField] protected IntegerData _startIndex = new IntegerData(0);
    
        [Tooltip("Select if the calling block should stop or continue executing commands, " +
            "or wait until the called block finishes.")]
        [FormerlySerializedAs("callMode")]
        [SerializeField] protected CallMode _callMode = CallMode.WaitUntilFinished;

        [SerializeField] [HideInInspector] [FormerlySerializedAs("targetBlockId")] 
        private ushort _targetBlockId;
        public ushort TargetBlockId => _targetBlockId;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_startLabel);
            _variableDataCache.Add(_startIndex);
        }

        public override void OnPreCut()
        {
            base.OnPreCut();
            RegisterTargetBlockId();
        }

        private void RegisterTargetBlockId()
        {
            _targetBlockId = _targetBlock != null ? 
                _targetBlock.ItemId : 
                (ushort)0;
        }
        #region Public members

        public override void OnEnter()
        {
            if (_targetBlock != null)
            {
                // Check if calling your own parent block
                bool callingOwnParent = ParentBlock != null && _targetBlock.Equals(ParentBlock);
                if (callingOwnParent)
                {
                    // Just ignore the callmode in this case, and jump to first command in list
                    Continue(0);
                    return;
                }

                if (_targetBlock.IsExecuting())
                {
                    Debug.LogWarning(_targetBlock.BlockName + " cannot be called/executed, it is already running.");
                    Continue();
                    return;
                }

                // Callback action for Wait Until Finished mode
                Action onComplete = null;
                if (_callMode == CallMode.WaitUntilFinished)
                {
                    onComplete = delegate {
                        Continue();
                    };
                }

                // Find the command index to start execution at
                int index = _startIndex;
                if (_startLabel.Value != "")
                {
                    int labelIndex = _targetBlock.GetLabelIndex(_startLabel.Value);
                    if (labelIndex != -1)
                    {
                        index = labelIndex;
                    }
                }

                if (_targetFlowchart == null ||
                    _targetFlowchart.Equals(GetFlowchart()))
                {
                    if (_callMode == CallMode.StopThenCall)
                    {
                        StopParentBlock();
                    }
                    StartCoroutine(_targetBlock.Execute(index, onComplete));
                }
                else
                {
                    if (_callMode == CallMode.StopThenCall)
                    {
                        StopParentBlock();
                    }
                    // Execute block in another Flowchart
                    _targetFlowchart.ExecuteBlock(_targetBlock, index, onComplete);
                }
            }

            if (_callMode == CallMode.Stop || _callMode == CallMode.Null)
            {
                StopParentBlock();
            }
            else if (_callMode == CallMode.Continue)
            {
                Continue();
            }
        }

        public override void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {
            if (_targetBlock != null)
            {
                connectedBlocks.Add(_targetBlock);
            }       
        }
        
        public override string GetSummary()
        {
            string summary = "";

            if (_targetBlock == null)
            {
                summary = "<None>";
            }
            else
            {
                summary = _targetBlock.BlockName;
            }

            summary += " : " + _callMode.ToString();

            return summary;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_startLabel.VarRef, variable) || base.HasReference(variable);
        }

        public bool MayCallBlock(Block block)
        {
            return block == _targetBlock;
        }

        #endregion

        protected override void OnValidate()
        {
            base.OnValidate();
            RegisterTargetBlockId();
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldStartIndex >= 0)
            {
                _startIndex.LiteralValue = _oldStartIndex;
                _oldStartIndex = -1;
            }
        }

        [FormerlySerializedAs("startIndex")]
        [SerializeField] protected int _oldStartIndex;

        protected override void DelayedOnValidate()
        {
            base.DelayedOnValidate();
            if (_callMode == CallMode.Null)
            {
                _callMode = CallMode.Stop;
            }
        }
    }
}