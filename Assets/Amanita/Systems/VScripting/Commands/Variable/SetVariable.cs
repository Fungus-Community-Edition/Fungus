using UnityEngine;
using System.Reflection;
using Type = System.Type;

namespace Amanita.VScripting.Commands
{
    /// <summary>
    /// Sets a Boolean, Integer, Float or String variable to a new value using a simple arithmetic operation. 
    /// The value can be a constant or reference another variable of the same type.
    /// </summary>
    [CommandInfo("Variable",
                 "Set Variable",
                 "Sets a Boolean, Integer, Float or String variable to a new value using a " +
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
            
#if UNITY_EDITOR
            // Reflection-based debug of anyVar.varRef internals
            //ShowAnyVarVarRefInternals(); // When this is commented out, this func returns "Error: no variable selected"
            void ShowAnyVarVarRefInternals()
            {
                try
                {
                    BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var anyVarObj = (object)anyVar;
                    if (anyVarObj != null)
                    {
                        var anyVarType = anyVarObj.GetType();
                        var varRefField = anyVarType.GetField("varRef", flags);
                        var varRef = varRefField != null ? varRefField.GetValue(anyVarObj) : null;
                        if (varRef != null)
                        {
                            var varRefType = varRef.GetType();

                            // itemId (private byte)
                            var itemIdField = varRefType.GetField("itemId", flags);
                            object itemIdObj = itemIdField != null ? itemIdField.GetValue(varRef) : null;
                            int itemIdValue = itemIdObj != null ? System.Convert.ToInt32(itemIdObj) : -1;

                            // VarOwner property
                            var varOwnerProp = varRefType.GetProperty("VarOwner", flags);
                            object varOwnerObj = varOwnerProp != null ? varOwnerProp.GetValue(varRef, null) : null;

                            // Backing serialized owners
                            var owningFcField = varRefType.GetField("owningFc", flags);
                            var owningVsaField = varRefType.GetField("owningVsa", flags);
                            var owningFc = owningFcField != null ? owningFcField.GetValue(varRef) as UnityEngine.Object : null;
                            var owningVsa = owningVsaField != null ? owningVsaField.GetValue(varRef) as UnityEngine.Object : null;

                            string ownerStr =
                                varOwnerObj is UnityEngine.Object uo ? $"{uo.GetType().Name} '{uo.name}'" :
                                varOwnerObj != null ? varOwnerObj.GetType().Name : "null";

                            string owningFcStr = owningFc != null ? $"{owningFc.GetType().Name} '{owningFc.name}'" : "null";
                            string owningVsaStr = owningVsa != null ? $"{owningVsa.GetType().Name} '{owningVsa.name}'" : "null";

                            Debug.Log($"[SetVariable.GetSummary] anyVar.varRef -> itemId = {itemIdValue},\n" +
                                $"VarOwner = {ownerStr},\n" +
                                $"owningFc = {owningFcStr},\n" +
                                $"owningVsa = {owningVsaStr}", this);
                        }
                        else
                        {
                            Debug.Log("[SetVariable.GetSummary] anyVar.varRef is null", this);
                        }
                    }
                    else
                    {
                        Debug.Log("[SetVariable.GetSummary] anyVar is null", this);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[SetVariable.GetSummary] Reflection debug failed: {ex.Message}", this);
                }
            }
#endif
            // Prefer resolving directly from the serialized reference to avoid stale cache
            var lhsVar = anyVar.LhsVariable;
            if (lhsVar == null)
            {
                // Try resolving from VariableReference if cache hasn’t caught up yet
                // (in case AnyVariableAndDataPair not yet refreshed in this repaint)
#if UNITY_EDITOR
                anyVar.RefreshVariableCacheHelper(GetFlowchart(), ref referencedVariables);
                lhsVar = anyVar.LhsVariable;
#endif
            }

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
