using UnityEngine;

namespace Amanita.VScripting.Commands
{
    /// <summary>
    /// Sets a Boolean, Integer, Float or String variable to a new value using a simple arithmetic operation. 
    /// The value can be a constant or reference another variable of the same type.
    /// </summary>
    [CommandInfo("Variable",
                 "Set Variable",
                 "Sets a Boolean, Integer, Float or String variable to a new value using a simple arithmetic operation. The value can be a constant or reference another variable of the same type.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class SetVariable : Command, ISerializationCallbackReceiver
    {
        [SerializeField] protected AnyVariableAndDataPair anyVar = new AnyVariableAndDataPair();
        
        [Tooltip("The type of math operation to be performed")]
        [SerializeField] protected SetOperator setOperator;
               
        protected virtual void DoSetOperation()
        {
            if (anyVar.LhsVariable == null)
            {
                return;
            }

            anyVar.SetOp(setOperator);
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
            if (anyVar.LhsVariable == null)
            {
                return "Error: Variable not selected";
            }

            string description = anyVar.LhsVariable.Key;
            description += " " + VariableUtil.GetSetOperatorDescription(setOperator) + " ";
            description += anyVar.GetDataDescription();

            return description;
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

            anyVar?.RefreshVariableCacheHelper(GetFlowchart(), ref referencedVariables);
        }
#endif
        #endregion Editor caches

        #region backwards compat

        [Tooltip("Variable to use in expression")]
        [VariableProperty]
        [SerializeField] protected Variable variable;

        public void OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }

        protected virtual void OnEnable()
        {
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
