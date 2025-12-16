using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Pairing of an AnyVariableData and an variable reference. Internal lookup for
    /// matching the right kind of variable with the correct data in the AnyVariableData.
    /// </summary>
    [Serializable]
    public class AnyVariableAndDataPair : ISerializationCallbackReceiver
    {
        [SerializeField] private VariableReference varRef = new VariableReference();
        [SerializeField] private AnyVariableData data = new AnyVariableData(); // RHS

        public AnyVariableData Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        public virtual IVariable LhsVariable
        {
            get
            {
                // Always derive from the serialized reference to avoid stale cache
                varRef?.Refresh();
                return varRef?.Variable;
            }
            set
            {
                varRef ??= new VariableReference();
                varRef.Variable = value;
            }
        }

        public bool HasReference(Variable variable)
        {
            // Only legacy comparison makes sense for this signature
            return ReferenceEquals(variable, LhsVariable) || data.HasReference(variable);
        }

#if UNITY_EDITOR
        public void RefreshVariableCacheHelper(Flowchart flowchart, ref IList<IVariable> referencedVariables)
        {
            var eff = LhsVariable;

            if (eff is IVariable<string> asStringVar &&
                asStringVar != null &&
                !string.IsNullOrEmpty(asStringVar.Value))
            {
                flowchart.DetermineSubstituteVariables(asStringVar.Value, referencedVariables);
            }

            string text = data.BoxedValue as string;
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

        // Important: consider legacy first, then managed, and unwrap pointers as needed
        protected virtual Type VarType
        {
            get
            {
                var eff = LhsVariable;
                if (eff == null)
                    return null;

                return eff.GetType();
            }
        }

        public bool Compare(CompareOperator compareOperator, ref bool compareResult)
        {
            var eff = LhsVariable;
            bool foundActions = TryGetTypeActionsFor(VarType, out var typeActions);

            if (foundActions)
            {
                compareResult = typeActions.CompareFunc(eff, data, compareOperator);
            }

            return foundActions;
        }

        public void SetOp(SetOperator setOperator)
        {
            var eff = LhsVariable;
            bool foundActions = TryGetTypeActionsFor(VarType, out VariableTypeActions typeActions);
            if (foundActions)
            {
                typeActions.SetFunc(eff, data, setOperator);
            }
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            varRef.Refresh();
        }
    }
}