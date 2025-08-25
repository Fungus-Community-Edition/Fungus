using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Pairing of an AnyVariableData and an variable reference. Internal lookup for
    /// making the right kind of variable with the correct data in the AnyVariableData.
    /// This is the primary mechanism for hiding the ugly need to match variable to
    /// correct data type so we can perform comparisons and operations.
    ///
    /// New types created need to be added to the list below and also to AllVariableTypes and
    /// AnyVariableData
    /// 
    /// Note to ensure use of RefreshVariableCacheHelper in commands, see SetVariable for
    /// example.
    /// </summary>
    [System.Serializable]
    public class AnyVariableAndDataPair : ISerializationCallbackReceiver
    {
        public virtual IVariable Variable
        {
            get { return variable; }
            set { variable = value as Variable; }
        }

        [VariableProperty()]
        [SerializeField] protected Variable variable;

        public AnyVariableData Data
        {
            get => data;
            set { data = value; }
        }

        [SerializeField, SerializeReference] protected AnyVariableData data = new AnyVariableData(); // Used as the right hand side in Set Variable

        public virtual void OnBeforeSerialize()
        {
            data.OnBeforeSerialize();
            data.VarRef = variable;
        }

        public virtual void OnAfterDeserialize()
        {
            data.OnAfterDeserialize();
            data.VarRef = variable;
        }

        public bool HasReference(Variable variable)
        {
            return variable == this.variable || data.HasReference(variable);
        }

#if UNITY_EDITOR
        public void RefreshVariableCacheHelper(Flowchart flowchart, ref List<Variable> referencedVariables)
        {
            if (variable is StringVariable asStringVar && 
                asStringVar != null && 
                !string.IsNullOrEmpty(asStringVar.Value))
                flowchart.DetermineSubstituteVariables(asStringVar.Value, referencedVariables);

            string text = data.Value as string;
            if (!string.IsNullOrEmpty(text))
            {
                flowchart.DetermineSubstituteVariables(text, referencedVariables);
            }
                
        }
#endif

        public string GetDataDescription()
        {
            bool success = TryGetTypeActionsFor(VarType, out var typeActions);
            if (success)
            {
                return typeActions.DescFunc(data);
            }
            return "Null";
        }

        protected static bool TryGetTypeActionsFor(Type varType, out VariableTypeActions result)
        {
            return VariableTypeRegistry.TryGetTypeActionsFor(varType, out result);
        }

        protected virtual Type VarType
        {
            get
            {
                if (variable != null)
                {
                    return variable.GetType();
                }

                return null;
            }
        }

        public bool Compare(CompareOperator compareOperator, ref bool compareResult)
        {
            bool foundActions = TryGetTypeActionsFor(VarType, out var typeActions);

            if (foundActions)
            {
                typeActions.CompareFunc(variable, data, compareOperator);
            }

            return foundActions;
        }

        public void SetOp(SetOperator setOperator)
        {
            bool foundActions = TryGetTypeActionsFor(VarType, out VariableTypeActions typeActions);
            if (foundActions)
            {
                typeActions.SetFunc(variable, data, setOperator);
            }
        }

    }
}