using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Move execution to a specific Label command in the same block.
    /// </summary>
    [CommandInfo("Flow", 
                 "Jump", 
                 "Move execution to a specific Label command in the same block")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Jump : Command
    {
        [Tooltip("Name of a label in this block to jump to")]
        [SerializeField] protected StringData _targetLabel = new StringData("");

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_targetLabel);
        }

        #region Public members

        public override void OnEnter()
        {
            if (_targetLabel.Value == "")
            {
                Continue();
                return;
            }

            var commandList = ParentBlock.CommandList;
            for (int i = 0; i < commandList.Count; i++)
            {
                var command = commandList[i];
                Label label = command as Label;
                if (label != null && label.Key == _targetLabel.Value)
                {
                    Continue(label.CommandIndex + 1);
                    return;
                }
            }

            // Label not found
            Debug.LogWarning("Label not found: " + _targetLabel.Value);
            Continue();
        }

        public override string GetSummary()
        {
            string result = "To " + _targetLabel.Value;
            if (_targetLabel.Value == "")
            {
                return "Error: No label selected";
            }

            if (_targetLabel.RepresentingVar)
            {
                result += $" ({_targetLabel.VarRef.Key})";
            }

            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.ConditionalLogic;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_targetLabel.VarRef, variable) ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("targetLabel")] public Label targetLabelOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (targetLabelOLD != null)
            {
                _targetLabel.Value = targetLabelOLD.Key;
                targetLabelOLD = null;
            }
        }

        #endregion
    }
}
