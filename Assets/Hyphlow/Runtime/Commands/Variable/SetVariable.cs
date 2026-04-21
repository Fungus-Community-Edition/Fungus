using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets a variable to a new value using a simple arithmetic operation. 
    /// The value can be a constant or reference another variable of the same type.
    /// </summary>
    [CommandInfo("Variable",
                 "Set Variable",
                 "Sets a Muscariable (or legacy Flowchart variable) to a new value using a " +
                "simple arithmetic operation. The value can be a constant or reference another " +
                "variable of the same type.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [MovedFrom("AtMycelia.Amanita.VScripting.Commands")]
    public class SetVariable : Command
    {
        [Tooltip("The type of math operation to be performed")]
        [FormerlySerializedAs("setOperator")]
        [SerializeField] protected SetOperator _setOperator;
        [FormerlySerializedAs("anyVar")]
        [SerializeField] protected AnyVariableAndDataPair _anyVar = new AnyVariableAndDataPair();
        // ^Contains both the LHS variable reference and the RHS data

#if UNITY_EDITOR
        public override bool NonStandardPaste => true;
#endif

        protected virtual void DoSetOperation()
        {
            if (_anyVar.LhsVariable == null)
            {
                return;
            }

            _anyVar.SetOp(_setOperator);
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_anyVar.Data);
        }

        #region Public members

        /// <summary>
        /// The type of math operation to be performed.
        /// </summary>
        public virtual SetOperator SetOperator { get { return _setOperator; } }

        public override void OnEnter()
        {
            DoSetOperation();

            Continue();
        }

        public override string GetSummary()
        {
            var lhsVar = _anyVar.LhsVariable;
            if (lhsVar == null)
            {
                return "Error: Variable not selected";
            }

            string setOperatorDesc = VariableUtil.GetSetOperatorDescription(_setOperator);
            string dataDesc = _anyVar.GetDataDescription();
            string description = $"{lhsVar.Key} {setOperatorDesc} {dataDesc}";
            // If the variable doesn't share an owner with us, we should make that clear
            // in the summary.
            bool varBelongsToSomethingElse = lhsVar.Owner != null && !ReferenceEquals(lhsVar.Owner, GetFlowchart());
            if (varBelongsToSomethingElse)
            {
                description = $"{lhsVar.Owner.Name}." + description;
            }

            return description;
        }

        public override bool HasReference(Variable variable)
        {
            return _anyVar.HasReference(variable);
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }

        #endregion

        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            _anyVar ??= new AnyVariableAndDataPair();
            _anyVar.RefreshVariableCacheHelper(GetFlowchart(), ref referencedVariables);
        }
#endif
        #endregion Editor caches

        #region backwards compat

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldVariable != null)
            {
                _anyVar.LhsVariable = _oldVariable;
                _oldVariable = null;
            }

            if (_oldAnyVar != null && _oldAnyVar.LhsVariable != null)
            {
                _anyVar.LhsVariable = _oldAnyVar.LhsVariable;
                _oldAnyVar = null;
            }

            _anyVar.Refresh();
        }

        [Tooltip("Variable to use in expression")]
        [VariableProperty]
        [FormerlySerializedAs("variable")]
        [SerializeField] protected Variable _oldVariable;

        [FormerlySerializedAs("anyVar")]
        [HideInInspector]
        [SerializeField] protected AnyVariableAndDataPair _oldAnyVar;

        protected override void OnEnable()
        {
            base.OnEnable();
            // We only want this check in the editor, not at runtime
            if (_oldVariable == null || Application.isPlaying)
            {
                return;
            }
            else
            {
                ApplyBackwardsCompatibility();
            }

            _oldVariable = null;
        }
        #endregion
    
    }
}
