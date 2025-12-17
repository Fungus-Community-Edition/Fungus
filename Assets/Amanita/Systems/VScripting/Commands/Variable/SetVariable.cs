using UnityEngine;

namespace Amanita.VScripting.Commands
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
    public class SetVariable : Command, ISerializationCallbackReceiver
    {
        [SerializeField] private VariableReference varToSet;
        [Tooltip("The type of math operation to be performed")]
        [SerializeField] protected SetOperator setOperator;
        [SerializeField] protected AnyVariableAndDataPair anyVar = new AnyVariableAndDataPair();
        
        protected virtual void DoSetOperation()
        {
            if (anyVar.LhsVariable == null)
            {
                return;
            }

            anyVar.SetOp(setOperator);
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(anyVar.Data);
        }

        #region Public members

        /// <summary>
        /// The type of math operation to be performed.
        /// </summary>
        public virtual SetOperator SetOperator { get { return setOperator; } }

        public override void OnEnter()
        {
            DoSetOperation();

            Continue();
        }

        public override string GetSummary()
        {
            // Prefer resolving directly from the serialized reference to avoid stale cache
            var lhsVar = anyVar.LhsVariable;
//            if (lhsVar == null)
//            {
//                // Try resolving from VariableReference if cache hasn’t caught up yet
//                // (in case AnyVariableAndDataPair not yet refreshed in this repaint)
//#if UNITY_EDITOR
//                anyVar.RefreshVariableCacheHelper(GetFlowchart(), ref referencedVariables);
//                lhsVar = anyVar.LhsVariable;
//#endif
//            }

            if (lhsVar == null)
            {
                return "Error: Variable not selected";
            }

            string description = lhsVar.Key;
            description += " " + VariableUtil.GetSetOperatorDescription(setOperator) + " ";
            description += anyVar.GetDataDescription();

            return description;
        }

        protected override void AssertOwnership()
        {
            base.AssertOwnership();
        }

        public override bool HasReference(Variable variable)
        {
            return anyVar.HasReference(variable);
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

            anyVar ??= new AnyVariableAndDataPair();
            anyVar.RefreshVariableCacheHelper(GetFlowchart(), ref referencedVariables);
        }
#endif
        #endregion Editor caches

        #region backwards compat

        [Tooltip("Variable to use in expression")]
        [VariableProperty]
        [SerializeField] protected Variable variable;

        public void OnBeforeSerialize()
        {
            anyVar.OnBeforeSerialize();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            //anyVar.OnAfterDeserialize();
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            // We only want this check in the editor, not at runtime
            if (variable == null || Application.isPlaying)
            {
                return;
            }
            else
            {
                anyVar.LhsVariable = variable;
            }

            variable = null;
        }
        #endregion
    
    }
}
