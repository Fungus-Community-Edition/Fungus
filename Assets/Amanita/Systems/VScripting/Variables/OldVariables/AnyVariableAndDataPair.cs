using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Pairing of an AnyVariableData and an variable reference. Internal lookup for
    /// making the right kind of variable with the correct data in the AnyVariableData.
    /// </summary>
    [Serializable]
    public class AnyVariableAndDataPair : ISerializationCallbackReceiver
    {
        public virtual IVariable Variable
        {
            get
            {
                if (legacyVariable != null)
                {
                    return legacyVariable;
                }

                return variable;
            }
            set
            {
                bool valIsLegacyVar = value is Variable legacyVar;
                if (valIsLegacyVar)
                {
                    legacyVariable = value as Variable;
                    variable = null; // ensure only one is authoritative
                }
                else
                {
                    variable = value;
                    legacyVariable = null;
                }
            }
        }

        [VariableProperty()]
        [SerializeReference] protected IVariable variable; // The lhs variable in Set Variable (managed)

        [VariableProperty()]
        [SerializeField] protected Variable legacyVariable; // The lhs variable in Set Variable (legacy MonoBehaviour)

        public AnyVariableData Data
        {
            get => data;
            set { data = value; }
        }

        [SerializeField] protected AnyVariableData data = new AnyVariableData();

        // Helper: decide which one is authoritative
        protected IVariable EffectiveVariable => legacyVariable != null ? legacyVariable : variable;

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            data.OnAfterDeserialize();

            var eff = EffectiveVariable;
            if (eff != null && data.VarRef == null)
            {
                data.SetFor(VarType, eff.ContentType);
            }
        }

        public bool HasReference(Variable variable)
        {
            // Only legacy comparison makes sense for this signature
            return ReferenceEquals(variable, this.legacyVariable) || data.HasReference(variable);
        }

#if UNITY_EDITOR
        public void RefreshVariableCacheHelper(Flowchart flowchart, ref IList<IVariable> referencedVariables)
        {
            var eff = EffectiveVariable;

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
                var eff = EffectiveVariable;
                if (eff == null)
                    return null;

                return eff.GetType();
            }
        }

        public bool Compare(CompareOperator compareOperator, ref bool compareResult)
        {
            var eff = EffectiveVariable;
            bool foundActions = TryGetTypeActionsFor(VarType, out var typeActions);

            if (foundActions)
            {
                compareResult = typeActions.CompareFunc(eff, data, compareOperator);
            }

            return foundActions;
        }

        public void SetOp(SetOperator setOperator)
        {
            var eff = EffectiveVariable;
            bool foundActions = TryGetTypeActionsFor(VarType, out VariableTypeActions typeActions);
            if (foundActions)
            {
                typeActions.SetFunc(eff, data, setOperator);
            }
        }
    }
}